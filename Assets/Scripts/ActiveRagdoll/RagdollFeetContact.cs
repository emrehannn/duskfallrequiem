using UnityEngine;

public class RagdollFeetContact : MonoBehaviour
{
    [SerializeField] private RagdollController RagdollPlayer;
    private const string terrain = "terrain";
    
    void OnCollisionEnter(Collision col)
    {
        if(!RagdollPlayer.isJumping && RagdollPlayer.inAir)
        {
            if(col.gameObject.layer == LayerMask.NameToLayer(terrain))
            {
                RagdollPlayer.PlayerLanded();
            }
        }
    }
}
