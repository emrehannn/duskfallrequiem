using UnityEngine;


public static class PlayerTarget

{

    public static Transform Instance { get; private set; }


    public static void SetTarget(Transform target)

    {

        Instance = target;

    }

}