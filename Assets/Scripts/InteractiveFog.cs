using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InteractiveFog : MonoBehaviour
{
    [Header("Base Properties")]
    [Range(1f, 100f)]
    public float width = 20f;
    
    [Range(1f, 100f)]
    public float depth = 20f;
    
    [Range(0.1f, 10f)]
    public float height = 1.2f;
    
    [Range(4, 100)]
    public int pointsPerSide = 20;
    
    [Header("Appearance")]
    [Range(0f, 1f)]
    public float fogDensity = 0.7f;
    
    [Range(0f, 1f)]
    public float fogOpacity = 0.8f;
    
    public Color fogColor = new Color(0.8f, 0.8f, 0.95f, 1f);
    
    public Gradient fogHeightGradient;
    
    [Header("Noise")]
    [Range(0.01f, 5f)]
    public float noiseScale = 0.8f;
    
    [Range(0.01f, 2f)]
    public float noiseSpeed = 0.15f;
    
    [Range(0f, 1f)]
    public float noiseInfluence = 0.4f;
    
    [Header("Dynamics")]
    [Range(0.1f, 5f)]
    public float interactionForce = 1.5f;
    
    [Range(0.1f, 5f)]
    public float interactionRadius = 2.0f;
    
    [Range(0.1f, 5f)]
    public float recoverySpeed = 1.2f;
    
    [Range(0f, 1f)]
    public float propagationFactor = 0.4f;
    
    public LayerMask interactionLayers = -1;
    
    // References
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material fogMaterial;
    
    // Mesh data
    private Mesh fogMesh;
    private Vector3[] baseVertices;
    private Vector3[] vertices;
    private Vector3[] vertexVelocities;
    private int[] pointIndices;
    private Color[] vertexColors; // Persistent array for vertex colors
    
    // Runtime variables
    private float noiseOffsetX;
    private float noiseOffsetY;
    private bool meshInitialized = false;
    
    void OnValidate()
    {
        if (meshInitialized && gameObject.activeInHierarchy)
        {
            RegenerateMesh();
        }
    }
    
    void Awake()
    {
        if (fogHeightGradient == null || fogHeightGradient.colorKeys.Length == 0)
        {
            // Create default gradient if none exists
            fogHeightGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(fogColor, 0f);
            colorKeys[1] = new GradientColorKey(new Color(fogColor.r, fogColor.g, fogColor.b, 0f), 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(fogOpacity, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);
            
            fogHeightGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    void Start()
    {
        InitializeFog();
    }
    
    void Update()
    {
        UpdateNoiseOffset();
        HandleInteractions();
        SimulateFogPhysics();
        UpdateMesh();
    }
    
    void InitializeFog()
    {
        // Get or add required components
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
            
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        // Create mesh
        fogMesh = new Mesh();
        fogMesh.name = "FogMesh";
        meshFilter.mesh = fogMesh;
        
        // Create material with proper shader for URP (or fallback to Standard)
        if (GraphicsSettings.currentRenderPipeline != null)
        {
            fogMaterial = new Material(Shader.Find("Custom/GroundFog"));
            fogMaterial.SetFloat("_Surface", 1); // 1 = transparent
            fogMaterial.SetFloat("_Blend", 0);    // 0 = alpha blending
            fogMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            fogMaterial.SetFloat("_Metallic", 0.0f);
            fogMaterial.SetFloat("_Smoothness", 0.2f);
            fogMaterial.renderQueue = (int)RenderQueue.Transparent;
        }
        else
        {
            fogMaterial = new Material(Shader.Find("Standard"));
            fogMaterial.SetFloat("_Mode", 3); // Transparent mode
            fogMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            fogMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            fogMaterial.SetInt("_ZWrite", 0);
            fogMaterial.DisableKeyword("_ALPHATEST_ON");
            fogMaterial.EnableKeyword("_ALPHABLEND_ON");
            fogMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            fogMaterial.renderQueue = (int)RenderQueue.Transparent;
        }
        
        meshRenderer.material = fogMaterial;
        
        // Generate the fog mesh grid
        RegenerateMesh();
        meshInitialized = true;
    }
    
    void RegenerateMesh()
    {
        if (fogMesh != null)
            fogMesh.Clear();
            
        int pointCount = pointsPerSide * pointsPerSide;
        vertices = new Vector3[pointCount];
        baseVertices = new Vector3[pointCount];
        vertexVelocities = new Vector3[pointCount];
        vertexColors = new Color[pointCount];
        pointIndices = new int[pointCount];
        Vector2[] uvs = new Vector2[pointCount];
        
        float xStep = width / (pointsPerSide - 1);
        float zStep = depth / (pointsPerSide - 1);
        
        for (int z = 0; z < pointsPerSide; z++)
        {
            for (int x = 0; x < pointsPerSide; x++)
            {
                int index = z * pointsPerSide + x;
                
                float xPos = x * xStep - width / 2f;
                float zPos = z * zStep - depth / 2f;
                
                float distanceFromCenter = Mathf.Sqrt(
                    Mathf.Pow(xPos / (width / 2f), 2) + 
                    Mathf.Pow(zPos / (depth / 2f), 2)
                );
                
                float baseHeight = height * (1f - distanceFromCenter * 0.8f * (1f - fogDensity));
                baseHeight = Mathf.Max(0.05f, baseHeight);
                
                Vector3 pos = new Vector3(xPos, baseHeight, zPos);
                vertices[index] = pos;
                baseVertices[index] = pos;
                vertexVelocities[index] = Vector3.zero;
                pointIndices[index] = index;
                uvs[index] = new Vector2((float)x / (pointsPerSide - 1), (float)z / (pointsPerSide - 1));
                float heightRatio = baseHeight / height;
                vertexColors[index] = fogHeightGradient.Evaluate(1f - heightRatio);
            }
        }
        
        int numTriangles = (pointsPerSide - 1) * (pointsPerSide - 1) * 2;
        int[] triangles = new int[numTriangles * 3];
        int triangleIndex = 0;
        
        for (int z = 0; z < pointsPerSide - 1; z++)
        {
            for (int x = 0; x < pointsPerSide - 1; x++)
            {
                int topLeft = z * pointsPerSide + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * pointsPerSide + x;
                int bottomRight = bottomLeft + 1;
                
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = bottomRight;
            }
        }
        
        fogMesh.vertices = vertices;
        fogMesh.triangles = triangles;
        fogMesh.uv = uvs;
        fogMesh.colors = vertexColors;
        fogMesh.RecalculateNormals();
        fogMesh.RecalculateBounds();
    }
    
    void UpdateNoiseOffset()
    {
        noiseOffsetX += noiseSpeed * Time.deltaTime;
        noiseOffsetY += noiseSpeed * 0.7f * Time.deltaTime;
        
        // For each vertex, sample Perlin noise (using computed grid UVs) and add to vertical velocity
        for (int i = 0; i < vertices.Length; i++)
        {
            int xIndex = i % pointsPerSide;
            int zIndex = i / pointsPerSide;
            float u = (float)xIndex / (pointsPerSide - 1);
            float v = (float)zIndex / (pointsPerSide - 1);
            
            float noise = Mathf.PerlinNoise(u * noiseScale + noiseOffsetX, v * noiseScale + noiseOffsetY) * 2f - 1f;
            vertexVelocities[i].y += noise * noiseInfluence * Time.deltaTime;
        }
    }
    
    void HandleInteractions()
    {
        // Find colliders overlapping the fog volume
        Bounds fogBounds = fogMesh.bounds;
        Vector3 fogCenter = transform.TransformPoint(fogBounds.center);
        Vector3 fogSize = new Vector3(width, height * 2f, depth);
        
        Collider[] colliders = Physics.OverlapBox(fogCenter, fogSize * 0.5f, transform.rotation, interactionLayers);
        
        foreach (var collider in colliders)
        {
            if (!collider.enabled || !collider.gameObject.activeInHierarchy)
                continue;
            
            Vector3 colliderCenter = collider.bounds.center;
            // Skip if the collider is above the fog
            if (colliderCenter.y - collider.bounds.extents.y > transform.position.y + height)
                continue;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVertex = transform.TransformPoint(vertices[i]);
                Vector2 vertexPos2D = new Vector2(worldVertex.x, worldVertex.z);
                Vector2 colliderPos2D = new Vector2(colliderCenter.x, colliderCenter.z);
                float distance = Vector2.Distance(vertexPos2D, colliderPos2D);
                
                float colliderRadius = Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.z);
                float effectiveRadius = interactionRadius + colliderRadius;
                float influence = 1f - Mathf.Clamp01(distance / effectiveRadius);
                
                if (influence > 0.01f)
                {
                    float force = influence * interactionForce;
                    Rigidbody rb = collider.GetComponent<Rigidbody>();
                    if (rb != null && rb.linearVelocity.magnitude > 0.1f)
                        force *= Mathf.Clamp01(rb.linearVelocity.magnitude / 5f) + 1f;
                    
                    // Apply vertical push and horizontal displacement away from the collider
                    vertexVelocities[i].y -= force * Time.deltaTime * 10f;
                    
                    if (distance > 0.01f)
                    {
                        Vector2 pushDir = (vertexPos2D - colliderPos2D).normalized;
                        vertexVelocities[i].x += pushDir.x * force * Time.deltaTime * 2f;
                        vertexVelocities[i].z += pushDir.y * force * Time.deltaTime * 2f;
                    }
                }
            }
        }
    }
    
    void SimulateFogPhysics()
    {
        // Use a simple Laplacian diffusion to smooth height differences between neighbors
        Vector3[] newVelocities = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float sumNeighborHeights = 0f;
            int neighborCount = 0;
            int x = i % pointsPerSide;
            int z = i / pointsPerSide;
            
            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0)
                        continue;
                    int nx = x + dx;
                    int nz = z + dz;
                    if (nx < 0 || nx >= pointsPerSide || nz < 0 || nz >= pointsPerSide)
                        continue;
                    int neighborIndex = nz * pointsPerSide + nx;
                    sumNeighborHeights += vertices[neighborIndex].y;
                    neighborCount++;
                }
            }
            
            float averageNeighborHeight = (neighborCount > 0) ? sumNeighborHeights / neighborCount : vertices[i].y;
            float diffusionForce = (averageNeighborHeight - vertices[i].y) * propagationFactor;
            newVelocities[i] = vertexVelocities[i];
            newVelocities[i].y += diffusionForce;
        }
        
        // Apply a spring force pulling each vertex back to its base height and then integrate velocity
        for (int i = 0; i < vertices.Length; i++)
        {
            float springForce = (baseVertices[i].y - vertices[i].y) * recoverySpeed;
            newVelocities[i].y += springForce * Time.deltaTime;
            newVelocities[i] *= 0.95f; // damping
            vertexVelocities[i] = newVelocities[i];
            
            vertices[i].y += vertexVelocities[i].y * Time.deltaTime;
            vertices[i].y = Mathf.Clamp(vertices[i].y, 0.01f, height * 1.5f);
            // Keep x and z fixed
            vertices[i].x = baseVertices[i].x;
            vertices[i].z = baseVertices[i].z;
        }
    }
    
    void UpdateMesh()
    {
        fogMesh.vertices = vertices;
        
        // Update vertex colors based on current height and variation from base height
        for (int i = 0; i < vertices.Length; i++)
        {
            float heightRatio = vertices[i].y / height;
            Color col = fogHeightGradient.Evaluate(1f - heightRatio);
            float heightVariance = Mathf.Abs(vertices[i].y - baseVertices[i].y) / height;
            col.a = Mathf.Clamp01(col.a + heightVariance * 0.5f);
            vertexColors[i] = col;
        }
        
        fogMesh.colors = vertexColors;
        fogMesh.RecalculateNormals();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw the fog bounds for visualization
        Gizmos.color = new Color(fogColor.r, fogColor.g, fogColor.b, 0.2f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.up * height * 0.5f, new Vector3(width, height, depth));
        
        // Draw the interaction radius (centered at the origin)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(Vector3.zero, interactionRadius);
    }
}
