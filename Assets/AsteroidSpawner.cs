using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AsteroidSpawner))]
public class AsteroidSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AsteroidSpawner spawner = (AsteroidSpawner)target;
        if (GUILayout.Button("Respawn Asteroids"))
        {
            spawner.RespawnAsteroids();
        }
    }
}

public class AsteroidSpawner : MonoBehaviour
{
    // Prefabs assigned via the Inspector
    public GameObject asteroidPrefab;
    public GameObject smallAsteroidPrefab;

    // Radius for collision checking when spawning to prevent overlapping objects
    [SerializeField] private float spawnCheckRadius = 5f;
    // Maximum attempts to find a non-colliding position per spawn
    [SerializeField] private int maxSpawnAttempts = 10;

    void Start()
    {
        // Start spawning asteroids after one frame delay
        StartCoroutine(SpawnAsteroids());
    }

    public void RespawnAsteroids()
    {
        // Clear old asteroids and spawn new ones.
        ClearOldAsteroids();
        StartCoroutine(SpawnAsteroids());
    }

    private IEnumerator SpawnAsteroids()
    {
        // Delay execution by one frame to ensure Zones has generated rings
        yield return null;

        // Find the Zones manager from the scene
        Zones zonesManager = FindFirstObjectByType<Zones>();
        if (zonesManager == null)
        {
            Debug.LogError("Zones manager not found in the scene.");
            yield break;
        }

        // Retrieve zones from the Zones manager
        List<GameObject> zones = zonesManager.GetZones();
        if (zones == null || zones.Count == 0)
        {
            Debug.LogWarning("No zones found to spawn asteroids in.");
            yield break;
        }

        // Sort zones by their calculated radius (ascending)
        List<GameObject> sortedZones = zones.OrderBy(zone => zone.transform.localScale.x * 0.5f).ToList();

        // Loop through each sorted zone
        for (int i = 0; i < sortedZones.Count; i++)
        {
            GameObject zone = sortedZones[i];
            float outerRadius = zone.transform.localScale.x * 0.5f;
            GameObject prefabToSpawn;
            int spawnCount;

            // Define spawn counts and prefab types per zone:
            // Zone 1 (i==0): 1 small asteroid 
            // Zone 2 (i==1): 7 small asteroids 
            // Zone 3 (i==2): 15 small asteroids 
            // Zone 4 (i==3): 7 normal asteroids 
            // Zone 5 (i==4): 12 normal asteroids
            if (i < 3)
            {
                prefabToSpawn = smallAsteroidPrefab;
                switch (i)
                {
                    case 0:
                        spawnCount = 1;
                        break;
                    case 1:
                        spawnCount = 7;
                        break;
                    case 2:
                        spawnCount = 15;
                        break;
                    default:
                        spawnCount = 1;
                        break;
                }
            }
            else
            {
                prefabToSpawn = asteroidPrefab;
                switch (i)
                {
                    case 3:
                        spawnCount = 7;
                        break;
                    case 4:
                        spawnCount = 12;
                        break;
                    default:
                        spawnCount = 1;
                        break;
                }
            }

            // Determine the inner bound so that spawns don't appear inside an inner zone.
            // For Zone 1, inner radius is 0; otherwise it's the previous zone's outer radius.
            float innerRadius = (i == 0) ? 0f : sortedZones[i - 1].transform.localScale.x * 0.5f;

            // Create or find a container for asteroids for this zone.
            string zoneContainerName = "Zone " + (i + 1);
            GameObject zoneContainer = GameObject.Find(zoneContainerName);
            if (zoneContainer == null)
            {
                zoneContainer = new GameObject(zoneContainerName);
            }

            // Spawn asteroids randomly within the annular area.
            for (int j = 0; j < spawnCount; j++)
            {
                int attempts = 0;
                bool spawned = false;

                while (attempts < maxSpawnAttempts && !spawned)
                {
                    attempts++;

                    // Use Utilities to get a point within the annulus defined by innerRadius and outerRadius.
                    Vector3 spawnPosition = Utilities.GetPointInCircle(zone.transform.position, innerRadius, outerRadius);

                    // Check for any colliders overlapping with the spawnPosition.
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(spawnPosition, spawnCheckRadius);
                    bool collisionFound = (hitColliders.Length > 0);

                    if (!collisionFound)
                    {
                        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, zoneContainer.transform);
                        spawned = true;
                    }
                }

                if (!spawned)
                {
                    Debug.LogWarning($"Failed to spawn asteroid in zone at {zone.transform.position} after {maxSpawnAttempts} attempts.");
                }
            }
        }
    }

    private void ClearOldAsteroids()
    {
        // Attempt to get the Zones manager from the scene.
        Zones zonesManager = FindFirstObjectByType<Zones>();
        if (zonesManager == null)
        {
            Debug.LogError("Zones manager not found in the scene. Cannot clear old asteroids.");
            return;
        }

        // Retrieve zones from the Zones manager.
        List<GameObject> zones = zonesManager.GetZones();
        if (zones == null)
            return;

        // For each zone, look for a container named "Zone i" and remove its children.
        for (int i = 0; i < zones.Count; i++)
        {
            string zoneContainerName = "Zone " + (i + 1);
            GameObject zoneContainer = GameObject.Find(zoneContainerName);
            if (zoneContainer != null)
            {
                // Destroy all children in the container.
                for (int j = zoneContainer.transform.childCount - 1; j >= 0; j--)
                {
                    GameObject child = zoneContainer.transform.GetChild(j).gameObject;
                    if (Application.isPlaying)
                        Destroy(child);
                    else
                        DestroyImmediate(child);
                }
            }
        }
    }
}
