using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [BoxGroup("Cluster/Particle Debugging (Scene view only)")]
    public bool DrawParticleLines = false;
    [BoxGroup("Cluster/Particle Debugging (Scene view only)")]
    public bool DrawClusterCircles = false;

    public Player player;

    public float PlayerAttractionStrength = 5f;
    public float ParticleDamage = 10f;

    public List<Cluster> Clusters = new List<Cluster>();

    public GameObject ParticlePrefab;
    public GameObject ClusterPrefab;

    [BoxGroup("Simulation Configuration")]
    [OnValueChanged("Restart")]
    public Vector2 ScreenSpace = new Vector2(32, 18);

    [BoxGroup("Simulation Configuration")]
    [HideInInspector]
    public Vector2 HalfScreenSpace;

    [ReadOnly] public int numberOfTypes;

    [BoxGroup("Simulation Configuration")]
    [Tooltip("Set to false to disable clusters evolving/dying over time.")]
    public bool RunGame = true;

    [BoxGroup("Particle Properties")]
    [Range(-5, 5)]
    public float RepulsionEffector = -3f;

    [BoxGroup("Particle Properties")]
    [Range(0, 1)]
    public float Dampening = 0.1f;

    [BoxGroup("Particle Properties")]
    [Range(0, 2)]
    public float Friction = 0.9f;

    [BoxGroup("Unity Settings")]
    [OnValueChanged("ChangeTimescale")]
    [Range(0, 5)]
    public float _timeScale = 1f;

    [BoxGroup("Force Parameters")]
    [MinMaxSlider(-5f, 5f)]
    public Vector2 InternalForceRange = new Vector2(2f, 5f);

    [BoxGroup("Force Parameters")]
    [MinMaxSlider(-5f, 5f)]
    public Vector2 ExternalForceRange = new Vector2(-5f, 5f);

    [BoxGroup("Force Parameters")]
    [MinMaxSlider(0.1f, 2f)]
    public Vector2 InternalMinDistanceRange = new Vector2(0.1f, 0.5f);

    [BoxGroup("Force Parameters")]
    [MinMaxSlider(1f, 5f)]
    public Vector2 ExternalMinDistanceRange = new Vector2(1f, 2f);

    [BoxGroup("Force Parameters")]
    [MinMaxSlider(0.5f, 5f)]
    public Vector2 InternalRadiusRange = new Vector2(0.5f, 2f);

    [BoxGroup("Force Parameters")]
    [MinMaxSlider(2f, 7f)]
    public Vector2 ExternalRadiusRange = new Vector2(2f, 7f);

    [BoxGroup("Force Parameters")]
    [Range(0f, 5f)]
    public float CohesionStrength = 2f;

    public int StartPopulation = 5;

    private int _runningClusterCount = 0;

    private void ChangeTimescale() // This is just used in the inspector to update the time scale when changed
    {
        if (!Application.isPlaying) return;
        
        Time.timeScale = _timeScale;
    }


    private void Awake()
    {
        numberOfTypes = Enum.GetNames(typeof(ParticleType)).Length;

        // Singleton initialization
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Assign the player reference if not set
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        // Ensure the particle and cluster prefabs are assigned
        if (ParticlePrefab == null || ClusterPrefab == null)
        {
            Debug.LogError("ParticlePrefab or ClusterPrefab is not assigned in the inspector!");
            return;
        }

        // Enable running in background
        Application.runInBackground = true;
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        // Input handling
        if (Input.GetKeyDown(KeyCode.R)) // Restart the simulation if the "R" key is pressed
        {
            Restart();
        }

        // Update all clusters
        foreach (Cluster cluster in Clusters)
        {
            cluster.UpdateCluster();
        }
    }

    // Method to spawn a mutated cluster based on a base cluster
    private void SpawnMutatedCluster(Cluster baseCluster)
    {
        Vector2 pos = GetRandomPointOnScreen();
        Cluster newCluster = Instantiate(ClusterPrefab, pos, Quaternion.identity).GetComponent<Cluster>();

        // Copy the base cluster's properties
        newCluster.MaxInternalRadii = baseCluster.MaxInternalRadii;
        newCluster.MaxExternalRadii = baseCluster.MaxExternalRadii;

        // Ensure _numTypes is set
        newCluster._numTypes = baseCluster._numTypes;

        // Copy and mutate the force matrices
        newCluster.InternalForces = MutateForceMatrix(baseCluster.InternalForces);
        newCluster.ExternalForces = MutateForceMatrix(baseCluster.ExternalForces);
        newCluster.InternalMins = MutateForceMatrix(baseCluster.InternalMins);
        newCluster.ExternalMins = MutateForceMatrix(baseCluster.ExternalMins);
        newCluster.InternalRadii = MutateForceMatrix(baseCluster.InternalRadii);
        newCluster.ExternalRadii = MutateForceMatrix(baseCluster.ExternalRadii);

        newCluster.Initialize(pos.x, pos.y, baseCluster.ParticleTypeCounts);
        newCluster.Id = _runningClusterCount++;
        newCluster.gameObject.name = $"Cluster (Mutated from {baseCluster.Id})";

        Clusters.Add(newCluster);
    }

    // Method to mutate a force matrix
    private Array2D<float> MutateForceMatrix(Array2D<float> matrix)
    {
        int size0 = matrix.GetLength(0);
        int size1 = matrix.GetLength(1);
        Array2D<float> mutatedMatrix = new Array2D<float>(size0, size1);

        for (int i = 0; i < size0; i++)
        {
            for (int j = 0; j < size1; j++)
            {
                float mutation = UnityEngine.Random.Range(-0.1f, 0.1f);
                mutatedMatrix[i, j] = matrix[i, j] + mutation;
            }
        }

        return mutatedMatrix;
    }

    // Method to create a new cluster
    public Cluster CreateCluster()
    {
        //Debug.Log("Creating new cluster.");
        Vector2 pos = GetRandomPointOnScreen();

        Cluster newCluster = Instantiate(ClusterPrefab, pos, Quaternion.identity).GetComponent<Cluster>();

        // Example for how to initialize a dictionary for spawning
        //Dictionary<ParticleType, int> defaultParticleCounts = new Dictionary<ParticleType, int>
        //{
        //    { ParticleType.Neutral, 3 },
        //    { ParticleType.Fire, 3 },
        //    { ParticleType.Defense, 3 },
        //    { ParticleType.Speed, 3 }
        //};

        newCluster.Initialize(pos.x, pos.y, 30);
        newCluster.Id = _runningClusterCount++;

        newCluster.gameObject.name = "Cluster";

        Clusters.Add(newCluster);

        return newCluster;
    }

    private void OnValidate()
    {
        UpdateClusterParameters();
    }

    // Method to update force parameters for all clusters
    public void UpdateClusterParameters()
    {
        foreach (var cluster in Clusters)
        {
            cluster.UpdateForceParameters();
        }
    }

    private void Restart()
    {
        // Clear all clusters and restart
        ClearClusters();
        Initialize();
    }

    private void Initialize()
    {
        if (!Application.isPlaying) return;

        // Calculate half screen space
        HalfScreenSpace = ScreenSpace * 0.5f;

        // Spawn initial clusters
        for (int i = 0; i < StartPopulation; i++)
        {
            CreateCluster();
        }
    }

    // Method to clear all clusters
    private void ClearClusters()
    {
        for (int i = Clusters.Count - 1; i >= 0; i--)
        {
            Cluster cluster = Clusters[i];
            Clusters.RemoveAt(i);
            Destroy(cluster.gameObject);
        }

        _runningClusterCount = 0;
    }

    // Method to get a random point on the screen
    public Vector3 GetRandomPointOnScreen(bool awayFromPlayer = true)
    {
        Vector3 newPos;

        do
        {
            newPos = new Vector3(
                UnityEngine.Random.Range(-HalfScreenSpace.x, HalfScreenSpace.x),
                UnityEngine.Random.Range(-HalfScreenSpace.y, HalfScreenSpace.y),
            0);
        }
        while (Vector2.Distance(newPos, player.transform.position) < 6 && awayFromPlayer); // Continue generating new positions if they are too close to the player

        return newPos;
    }
}