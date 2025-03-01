using UnityEngine;

[System.Serializable]
public class VirtualEnemy
{
    public Vector3 position;

    public bool isReal;

    public int enemyTypeIndex;

    public int layer; // Add this
    public float speed = 0.7f; //lowPhysicsSpeed
    public GameObject gameObject;

    // Update this when enemy dies or layer changes

    public void SetLayer(int newLayer)

    {

        layer = newLayer;

    }




    public VirtualEnemy(Vector3 pos, int typeIndex)
    {
        position = pos;
        enemyTypeIndex = typeIndex;
        isReal = false;
        gameObject = null;

    }

    public void UpdatePosition(Vector3 playerPosition)
    {
        Vector3 direction = (playerPosition - position).normalized;
        direction.y = 0;
        position += direction * speed * Time.fixedDeltaTime;
    }
}