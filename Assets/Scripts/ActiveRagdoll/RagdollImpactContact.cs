using UnityEngine;

public class RagdollImpactContact : MonoBehaviour
{
    public RagdollController ragdollController;

    void OnCollisionEnter(Collision col)
    {
         if(ragdollController.isDead) return; 
        if (ragdollController.canBeKnockoutByImpact && col.relativeVelocity.magnitude > ragdollController.requiredForceToBeKO)
        {
            ragdollController.ActivateRagdoll();
        }
    }
}
