Shader "Custom/GroundFog"
{
    Properties
    {
        _Color ("Color", Color) = (0.8, 0.8, 0.8, 0.8)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Range(0.01, 10)) = 1.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.3
        _NoiseSpeed ("Noise Speed", Range(0, 2)) = 0.2
        _NoiseOffset ("Noise Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 noiseUV : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _Color;
            float _NoiseScale;
            float _NoiseStrength;
            float4 _NoiseOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.noiseUV = v.uv * _NoiseScale + _NoiseOffset.xy;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 noise = tex2D(_NoiseTex, i.noiseUV);
                fixed4 col = _Color * i.color;
                col.a *= lerp(1, noise.r, _NoiseStrength);
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/VertexLit"
}
