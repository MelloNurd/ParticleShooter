using UnityEngine;

public class ClusterSpawning : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private GameObject player;
    private Zones zones;

    void Start()
    {
        player = Player.Instance.gameObject;
        zones = Zones.Instance;

        if(player == null || zones == null)
        {
            Debug.LogError("Player or Zones instance is null. Make sure they are initialized before this script runs.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
