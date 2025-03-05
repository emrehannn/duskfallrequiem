using UnityEngine;

public class SpringLookupTable : MonoBehaviour
{
    public static SpringLookupTable Instance { get; private set; }

    private float[,] forceLookupTable;
    [SerializeField] private int tableResolution = 50;
    [SerializeField] private float maxHeightError = 1f;
    [SerializeField] private float maxVelocity = 5f;
    [SerializeField] private float heightStiffness = 20f; // Spring constant (K)
    [SerializeField] private float heightDamping = 5f; // Damping coefficient (D)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GenerateForceLookupTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void GenerateForceLookupTable()
    {
        forceLookupTable = new float[tableResolution, tableResolution];

        for (int i = 0; i < tableResolution; i++)
        {
            for (int j = 0; j < tableResolution; j++)
            {
                float heightError = Mathf.Lerp(-maxHeightError, maxHeightError, i / (float)(tableResolution - 1));
                float verticalVelocity = Mathf.Lerp(-maxVelocity, maxVelocity, j / (float)(tableResolution - 1));

                float force = heightError * heightStiffness - verticalVelocity * heightDamping;
                forceLookupTable[i, j] = force;
            }
        }
    }

    public float GetSpringForce(float heightError, float verticalVelocity, float stiffness, float damping)
{
    // Look up the base force from the table (approximates the response)
    int heightIndex = Mathf.RoundToInt(Mathf.InverseLerp(-maxHeightError, maxHeightError, heightError) * (tableResolution - 1));
    int velocityIndex = Mathf.RoundToInt(Mathf.InverseLerp(-maxVelocity, maxVelocity, verticalVelocity) * (tableResolution - 1));

    heightIndex = Mathf.Clamp(heightIndex, 0, tableResolution - 1);
    velocityIndex = Mathf.Clamp(velocityIndex, 0, tableResolution - 1);

    float baseForce = forceLookupTable[heightIndex, velocityIndex];

    // Apply per-enemy stiffness and damping
    return heightError * stiffness - verticalVelocity * damping;
}

}
