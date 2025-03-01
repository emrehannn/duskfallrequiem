using UnityEngine;
public class WeaponAxe : WeaponBase
{
    private void Awake()
    {
        useMouseTracking = false;
        useConstantOrbit = true;
        useSelfRotation = true;
    }
}