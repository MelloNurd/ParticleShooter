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

    private GameObject astZonePar;
    List<GameObject> asteroids = new List<GameObject>();

    // Radius for collision checking when spawning to prevent overlapping objects
    [SerializeField] private float spawnCheckRadius = 5f;
    // Maximum attempts to find a non-colliding position per spawn
    [SerializeField] private int maxSpawnAttempts = 10;

    void Start()
    {
        astZonePar = new GameObject("Asteroid Zone Parent");
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

        // Loop through each sorted zone
        for (int i = 0; i < zones.Count; i++)
        {
            GameObject zone = zones[i];
            float outerRadius = Zones.Instance.ringRadiuses[i];
            float innerRadius = 0;
            if (i - 1 >= 0)
            {
                innerRadius = Zones.Instance.ringRadiuses[i - 1];
            }
            GameObject prefabToSpawn;
            int spawnCount;

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

            // Create or find a container for asteroids for this zone.
            string zoneContainerName = "Zone " + (i);
            GameObject zoneContainer = new GameObject(zoneContainerName);
            zoneContainer.transform.parent = astZonePar.transform;

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
                        GameObject thisAsteroid = Instantiate(prefabToSpawn, spawnPosition, Quaternion.Euler(0, 0, Random.Range(0f, 360f)), zoneContainer.transform);
                        asteroids.Add(thisAsteroid);
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
        while(asteroids.Count > 0)
        {
            GameObject asteroid = asteroids[0];
            asteroids.RemoveAt(0);
            Destroy(asteroid);
        }
    }
}
