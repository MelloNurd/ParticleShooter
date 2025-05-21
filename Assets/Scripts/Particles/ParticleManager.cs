using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using Unity.Entities;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    public Entity ParticleEntityPrefab;

    public List<Cluster> Clusters = new List<Cluster>();

    public Player player;

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

        // Enable running in background
        Application.runInBackground = true;
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (ClusterSpawning.Instance == null || !ClusterSpawning.Instance.FinishedSpawning) return;

        // Update all clusters
        foreach (Cluster cluster in Clusters)
        {
            cluster.UpdateCluster();
        }
    }

    private void Initialize()
    {
        if (!Application.isPlaying) return;

        // Calculate half screen space
        HalfScreenSpace = ScreenSpace * 0.5f;
    }
}