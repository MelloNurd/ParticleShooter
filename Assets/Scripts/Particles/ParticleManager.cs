using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    public Player player;

    public GameObject ParticlePrefab;
    public GameObject ClusterPrefab;

    [ReadOnly] public int RunningClusterCount = 0;
    [ReadOnly] public int numberOfTypes;

    //////////////////////////

    [BoxGroup("Cluster/Particle Debugging (Scene view only)")]
    public bool DrawParticleLines = false;
    [BoxGroup("Cluster/Particle Debugging (Scene view only)")]
    public bool DrawClusterCircles = false;

    //////////////////////////

    [BoxGroup("Simulation Configuration")]
    [OnValueChanged("Restart")]
    public Vector2 ScreenSpace = new Vector2(32, 18);

    [BoxGroup("Simulation Configuration")]
    [HideInInspector]
    public Vector2 HalfScreenSpace;

    [BoxGroup("Simulation Configuration")]
    public int StartPopulation = 5;
    
    //////////////////////////

    [BoxGroup("Particle Properties")]
    [Range(-5, 5)]
    public float RepulsionEffector = -3f;

    [BoxGroup("Particle Properties")]
    [Range(0, 1)]
    public float Dampening = 0.1f;

    [BoxGroup("Particle Properties")]
    [Range(0, 2)]
    public float Friction = 0.9f;

    [BoxGroup("Particle Properties")]
    public float ParticleDamage = 10f;

    //////////////////////////
    
    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 InternalForceRange = new Vector2(2f, 5f);

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [MinMaxSlider(-5f, 5f)]
    public Vector2 ExternalForceRange = new Vector2(-5f, 5f);

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [MinMaxSlider(0.1f, 2f)]
    public Vector2 InternalMinDistanceRange = new Vector2(0.1f, 0.5f);

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 ExternalMinDistanceRange = new Vector2(1f, 2f);

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 InternalRadiusRange = new Vector2(0.5f, 2f);

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [MinMaxSlider(2f, 7f)]
    public Vector2 ExternalRadiusRange = new Vector2(2f, 7f);

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [Range(0f, 5f)]
    public float CohesionStrength = 2f;

    [BoxGroup("Force Parameters")]
    [OnValueChanged("UpdateClusterValuesRuntime")]
    [Range(0.1f, 10f)]
    public float ForceMultiplier = 1f;

    //////////////////////////
    
    [BoxGroup("Unity Settings")]
    [OnValueChanged("ChangeTimescale")]
    [Range(0, 5)]
    public float _timeScale = 1f;

    //////////////////////////

    private GameObject _clusterParent;

    private void ChangeTimescale() // This is just used in the inspector to update the time scale when changed
    {
        if (!Application.isPlaying) return;
        
        Time.timeScale = _timeScale;
    }

    [Button("Regenerate Cluster Values")]
    private void UpdateClusterValuesRuntime() // This is for an inspector value change, it is not called in code anywhere
    {
        foreach(Cluster cluster in Clusters)
        {
            cluster.InitializeForceMatrices();
        }
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
        _clusterParent = new GameObject("Cluster Parent");
        
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
        //for (int i = 0; i < StartPopulation; i++)
        //{
        //    CreateCluster();
        //}
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

        RunningClusterCount = 0;
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