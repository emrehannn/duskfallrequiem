using UnityEngine;

public class PlayerManager : MonoBehaviour 
{
    private static PlayerManager _instance;
    private Transform playerTransform;

    public static PlayerManager Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerManager");
                    _instance = go.AddComponent<PlayerManager>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public Transform GetPlayer()
    {
        return playerTransform;
    }

    public void SetPlayer(Transform player)
    {
        playerTransform = player;
        Debug.Log($"Player set in PlayerManager: {(player != null ? player.name : "null")}");
    }
}