using Mono.Cecil;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class ClusterSpawning : MonoBehaviour
{
    public static ClusterSpawning Instance { get; private set; }

    public bool FinishedSpawning { get; private set; } = false;

    public float DespawnTimeOffscreen = 10f;

    public GameObject ClusterPrefab;
    public GameObject ParticlePrefab;

    public Cluster lastDespawned;
    private int _maxClustersOnScreen = 30;

    private GameObject _player;
    private Rigidbody2D _playerRb;
    private Zones _zones;

    private List<SerializedDictionary<ParticleType, int>> _clusterDefinitions = new();

    private GameObject _clusterParent;

    private void Awake()
    {
        // Singleton Implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // Ensure the particle and cluster prefabs are assigned
        if (ParticlePrefab == null || ClusterPrefab == null)
        {
            Debug.LogError("ParticlePrefab or ClusterPrefab is not assigned in the inspector!");
            return;
        }
    }

    void Start()
    {
        _player = Player.Instance.gameObject;
        _playerRb = _player.GetComponent<Rigidbody2D>();
        _zones = Zones.Instance;

        if(_player == null || _zones == null)
        {
            Debug.LogError("Player or Zones instance is null. Make sure they are initialized before this script runs.");
            return;
        }

        _clusterParent = new GameObject("Cluster Parent");

        InitializeClusterTypes();
        Initialize();

        FinishedSpawning = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if the player is moving, and if the number of clusters is less than the max allowed
        // If so, small change to spawn a new cluster
        if (_playerRb.linearVelocity.magnitude > 0.1f && ParticleManager.Instance.Clusters.Count < _maxClustersOnScreen)
        {
            if (Random.Range(0, 150) > 1) return;

            Vector3 moveDir;
            if (Player.Instance.movingForwards) 
                moveDir = _player.transform.up;
            else if (Player.Instance.movingBackwards) 
                moveDir = -_player.transform.up;
            else 
                return;

            Vector3 randomPos;
            int threshold = 0;
            do
            {
                randomPos = Utilities.GetRandomPointOffScreen(1, 0.5f);
                threshold++;
            } // The Dot basically checks if the random position is in front of the player
            while (Vector3.Dot(moveDir, (randomPos - _player.transform.position).normalized) < 0.5f && threshold < 500);

            if(threshold >= 500)
            {
                Debug.LogWarning("Threshold reached while trying to find a point in circle.");
                return;
            }

            if(lastDespawned == null)
            {
                CreateCluster(randomPos);
            }
            else
            {
                lastDespawned.Reproduce(randomPos);
            }
        }
    }

    private void InitializeClusterTypes()
    {
        _clusterDefinitions.Clear();

        int? zoneCount = Zones.Instance.NumberOfZones;
        if(zoneCount == null || zoneCount == -1)
        {
            Debug.LogError("Zone count is invalid. Make sure the Zones instance is initialized before this script runs.");
            return;
        }

        int baseCount = 10;
        int zoneMultiplier = 8;

        for (int i = 0; i < zoneCount; i++)
        {
            _clusterDefinitions.Add(GenerateRandomCluster(baseCount + i*zoneMultiplier));
        }
    }

    private void Initialize()
    {
        //for (int i = 0; i < startClusterCount; i++)
        //{
        //    Vector3 randomPos = Utilities.GetPointInCircle(
        //        Player.Instance.transform.position,
        //        Player.Instance.minInteractionRadius,
        //        Player.Instance.minInteractionRadius * 1.5f
        //    );

        //    Cluster cluster = CreateCluster(randomPos);
        //}
    }

    private SerializedDictionary<ParticleType, int> ClusterTypeByZone(Vector3 pos)
    {
        int zoneIndex = Zones.GetCurrentZone(pos);
        if (zoneIndex == -1)
        {
            Debug.LogError("Zone index is invalid. Make sure the Zones instance is initialized before this script runs.");
            return null;
        }

        return _clusterDefinitions[zoneIndex];
    }

    // Method to create a new cluster
    public Cluster CreateCluster(Vector3 position)
    {
        int count = ParticleManager.Instance.Clusters.Count;

        Cluster newCluster = Instantiate(ClusterPrefab, position, Quaternion.identity, _clusterParent.transform).GetComponent<Cluster>();
        newCluster.gameObject.name = $"Cluster {++count}";

        // Example for how to initialize a dictionary for spawning
        //SerializedDictionary<ParticleType, int> defaultParticleCounts = new SerializedDictionary<ParticleType, int>
        //{
        //    { ParticleType.Neutral, 3 },
        //    { ParticleType.Fire, 1 },
        //    { ParticleType.Defense, 5 },
        //    { ParticleType.Speed, 3 }
        //};

        var clusterType = ClusterTypeByZone(position);
        if(clusterType == null)
        {
            Debug.LogError("Cluster type is null. Make sure the ClusterTypeByZone method is returning a valid dictionary.");
            return null;
        }

        newCluster.Id = count;
        newCluster.Initialize(position.x, position.y, clusterType);

        ParticleManager.Instance.Clusters.Add(newCluster);

        return newCluster;
    }

    private SerializedDictionary<ParticleType, int> GenerateRandomCluster(int particleCount)
    {
        SerializedDictionary<ParticleType, int> particleCounts = new SerializedDictionary<ParticleType, int>();
        
        for(int i = 0; i < particleCount; i++)
        {
            ParticleType randomType = (ParticleType)Random.Range(0, System.Enum.GetValues(typeof(ParticleType)).Length);

            if (particleCounts.ContainsKey(randomType))
            {
                particleCounts[randomType]++;
            }
            else
            {
                particleCounts[randomType] = 1;
            }
        }

        return particleCounts;
    }

}
