/*
 * Stripped & optimized version of the analytical light calculations
*/

#ifndef MICRO_AVL_INCLUDED
#define MICRO_AVL_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

inline bool IS(float3 rO, float3 rD, float3 sO, float sR, out float2 i)
{
    float a = dot(rD, rD);
    float3 s0_r0 = rO - sO;
    float b = 2.0 * dot(rD, s0_r0);
    float c = dot(s0_r0, s0_r0) - sR * sR;
    float d = b * b - 4.0 * a * c;
    if (d < 0.0)
    {
        i = 0;
        return false;
    }
    float dR = sqrt(d);
    i = float2(-b - dR, -b + dR) / (2.0 * a);
    return true;
}

inline float SPI1D(float x0, float y0, float A, float l)
{
    float J = rsqrt(l + y0 * y0);
    float b = x0 * J;
    float a = b + A * J;
    return (FastATan(a) - FastATan(b)) * J;
}

inline float SIE(float i, float rL, float lR, float lS)
{
    lS = max(lS, 0.0001);
    float invS = 1.0 / lS;
    float r2 = lR * lR;
    float eI = rL / (r2 + lS);
    i -= eI;
    float a0max = invS;
    float amax = invS - 1.0 / (r2 + lS);
    i *= a0max / amax;
    return i;
}

float CALP(float3 wsRO, float3 wsRD, float3 wsLO, float rL, float lR, float lS)
{
    float3x3 lsB;
    lsB[0] = wsRD;
    lsB[2] = normalize(cross(wsRD, wsLO - wsRO));
    lsB[1] = cross(lsB[0], lsB[2]);
    float3 wsV = wsRO - wsLO;
    float2 lsRO = float2(dot(lsB[0], wsV), dot(lsB[1], wsV));
    float aL = SPI1D(lsRO.x, lsRO.y, rL, lS);
    aL = SIE(aL, rL, lR, lS);
    return max(0.0, aL);
}

float CMPL(float3 wsCP, float3 wsVD, float3 wsLO, float eD, float lR, float lS)
{
    float2 dR = float2(0.0, eD);
    float2 lI;
    bool fSI = IS(wsCP, wsVD, wsLO, lR, lI);
    if (!fSI) return 0.0;
    dR.x = max(dR.x, lI.x);
    dR.y = min(dR.y, lI.y);
    dR.x = min(dR.x, dR.y);
    dR = max(dR, 0.0);
    float3 wsRO = wsCP + wsVD * dR.x;
    float rL = dR.y - dR.x;
    return CALP(wsRO, wsVD, wsLO, rL, lR, lS);
}

void CalculateMicroAVLPointLight_float(float3 wsCameraPosition, float3 wsViewDirection, float3 wsLightOrigin, float eyeDepth, float lightRange, float lightScattering, out float result)
{
    result = CMPL(wsCameraPosition, wsViewDirection, wsLightOrigin, eyeDepth, lightRange, lightScattering);
}

#endif