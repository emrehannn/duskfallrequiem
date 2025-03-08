using UnityEngine;

public class RagdollIterations : MonoBehaviour
{
    [SerializeField] private int solverIterations = 6;
    [SerializeField] private int velocityIterations = 2;

    void Start()
    {
        // Get all rigidbodies in children
        Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>();

        // Set iterations for each rigidbody
        foreach (Rigidbody rb in childRigidbodies)
        {
            rb.solverIterations = solverIterations;
            rb.solverVelocityIterations = velocityIterations;
        }
    }
}