using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ClusterSpawning : MonoBehaviour
{
    public static ClusterSpawning Instance { get; private set; }

    public List<Cluster> Clusters = new List<Cluster>();
    public bool FinishedSpawning = false;

    private int startClusterCount = 5;
    private int maxClustersOnScreen = 20;

    private GameObject player;
    private Zones zones;

    private List<Dictionary<ParticleType, int>> clusterDefinitions = new();

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
    }

    void Start()
    {
        player = Player.Instance.gameObject;
        zones = Zones.Instance;

        if(player == null || zones == null)
        {
            Debug.LogError("Player or Zones instance is null. Make sure they are initialized before this script runs.");
            return;
        }

        InitializeClusterList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeClusterList()
    {
        clusterDefinitions.Clear();

        int? zoneCount = Zones.Instance.NumberOfZones;
        if(zoneCount == null || zoneCount == -1)
        {
            Debug.LogError("Zone count is invalid. Make sure the Zones instance is initialized before this script runs.");
            return;
        }

        for(int i = 0; i < zoneCount; i++)
        {
            clusterDefinitions.Add(GenerateRandomCluster((i+1)*5));
        }
    }

    //private void Initialize()
    //{
    //    // Initialize clusters
    //    for (int i = 0; i < startClusterCount; i++)
    //    {
    //        cluster.Initialize(clusterDefinition);
    //    }
    //}

    //// Method to create a new cluster
    //public Cluster CreateCluster()
    //{
    //    //Debug.Log("Creating new cluster.");
    //    Vector2 pos = GetRandomPointOnScreen();

    //    Cluster newCluster = Instantiate(ClusterPrefab, pos, Quaternion.identity, _clusterParent.transform).GetComponent<Cluster>();

    //    // Example for how to initialize a dictionary for spawning
    //    //Dictionary<ParticleType, int> defaultParticleCounts = new Dictionary<ParticleType, int>
    //    //{
    //    //    { ParticleType.Neutral, 3 },
    //    //    { ParticleType.Fire, 3 },
    //    //    { ParticleType.Defense, 3 },
    //    //    { ParticleType.Speed, 3 }
    //    //};

    //    newCluster.Initialize(pos.x, pos.y, 30);
    //    newCluster.Id = RunningClusterCount++;

    //    newCluster.gameObject.name = $"Cluster {newCluster.Id}";

    //    Clusters.Add(newCluster);

    //    return newCluster;
    //}

    private Dictionary<ParticleType, int> GenerateRandomCluster(int particleCount)
    {
        Dictionary<ParticleType, int> particleCounts = new Dictionary<ParticleType, int>();
        
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
