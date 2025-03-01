using UnityEngine;

public class Wand : WeaponBase
{
    protected override void Update()
    {
        if (isDead || player == null) return;

        if (PlayerHealth.isPlayerDead && !isDead)
        {
            HandleDeath();
            return;
        }

        UpdateTipRotation();
        
        // Fixed position relative to player's right side
        Vector3 fixedPosition = player.position + 
                              (player.right * hiltOffset) + 
                              (Vector3.up * heightOffset);
        
        // Get mouse position for rotation
        Plane characterPlane = new Plane(Vector3.up, player.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (characterPlane.Raycast(ray, out float distance))
        {
            Vector3 lookAtPoint = ray.GetPoint(distance);
            // Calculate rotation to face mouse
            Vector3 direction = (lookAtPoint - fixedPosition).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction) * tipRotationOffset * 
                                      Quaternion.Euler(0, additionalRotation, 0);
            
            // Apply position and rotation
            transform.position = fixedPosition;
            transform.rotation = targetRotation;
        }
    }
}