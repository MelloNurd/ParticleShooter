using NaughtyAttributes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using static Unity.Entities.SystemBaseDelegates;

public struct SimSettings : IComponentData
{
    public int NumberOfParticles;
    public int NumberOfTypes;
    public float Friction;
    public float Dampening;
    public float RepulsionEffector;
    public float2 ScreenSpace;
    public float2 HalfScreenSpace;

    public Entity ForcesBuffer;
    public Entity MinDistancesBuffer;
    public Entity RadiiBuffer;
}

// Tag component to control respawning
public struct RespawnParticles : IComponentData { }

public class SettingsLoader : MonoBehaviour
{
    public static SettingsLoader Instance { get; private set; }
    [Header("Simulation Configuration")] ////////////////////////////////////////////////////////////////
    public Vector2 screenSpace = new Vector2(32, 18);
    [UnityEngine.Range(1, 9999)] public int numberOfParticles = 1000;
    [UnityEngine.Range(1, 32)] public int numberOfTypes = 5;

    [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
    [MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _forcesRange = new Vector2(0.3f, 1f);
    [MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _minDistancesRange = new Vector2(1f, 3f);
    [MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _radiiRange = new Vector2(3f, 5f);

    [UnityEngine.Range(-5, 5)] public float repulsion { get; set; } = -5f;
    [UnityEngine.Range(0, 2)] public float friction { get; set; } = 0.95f;
    [UnityEngine.Range(0, 1)] public float dampening { get; set; } = 0.5f;

    [Header("Unity Settings")] /////////////////////////////////////////////////////////////////////
    [UnityEngine.Range(0, 5)][SerializeField] public float _timeScale = 1f;

    // References to entities we create
    private Entity _forcesEntity;
    private Entity _minDistEntity;
    private Entity _radiiEntity;
    private Entity _simSettingsEntity;
    private Entity _respawnFlagEntity;

    // Track previous values to detect meaningful changes
    private int _prevParticleCount;
    private int _prevTypeCount;

    // Flag to track when settings need to be updated
    private bool _settingsDirty = false;
    private bool _needsRespawn = false;

    float startScreenSpaceX;
    float startScreenSpaceY;

    private UnityEvent<float, float> test = new();
    private UnityEvent<float> test2 = new();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of SettingsLoader exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        InitializeSettings();
        _prevParticleCount = numberOfParticles;
        _prevTypeCount = numberOfTypes;

        startScreenSpaceX = screenSpace.x;
        startScreenSpaceY = screenSpace.y;
    }

    void Update()
    {
        // Update timeScale globally
        Time.timeScale = _timeScale;

        // Check if settings need to be updated
        if (_settingsDirty)
        {
            // Check if we need to fully restart the simulation
            if (_needsRespawn)
            {
                RestartSimulation();
                _needsRespawn = false;
            }
            else
            {
                UpdateSettings();
            }
            _settingsDirty = false;
        }
    }

    private void InitializeSettings()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        _forcesEntity = em.CreateEntity(typeof(FloatBuffer));
        _minDistEntity = em.CreateEntity(typeof(FloatBuffer));
        _radiiEntity = em.CreateEntity(typeof(FloatBuffer));

        // Create respawn flag entity (will be used to signal respawn)
        _respawnFlagEntity = em.CreateEntity();

        _simSettingsEntity = em.CreateEntity();
        em.AddComponentData(_simSettingsEntity, new SimSettings
        {
            NumberOfParticles = numberOfParticles,
            NumberOfTypes = numberOfTypes,
            Friction = friction,
            Dampening = dampening,
            RepulsionEffector = repulsion,
            ScreenSpace = screenSpace,
            HalfScreenSpace = screenSpace * 0.5f,
            ForcesBuffer = _forcesEntity,
            MinDistancesBuffer = _minDistEntity,
            RadiiBuffer = _radiiEntity
        });

        UpdateBuffers();
    }

    private void UpdateSettings()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Update the main settings component
        em.SetComponentData(_simSettingsEntity, new SimSettings
        {
            NumberOfParticles = numberOfParticles,
            NumberOfTypes = numberOfTypes,
            Friction = friction,
            Dampening = dampening,
            RepulsionEffector = repulsion,
            ScreenSpace = screenSpace,
            HalfScreenSpace = screenSpace * 0.5f,
            ForcesBuffer = _forcesEntity,
            MinDistancesBuffer = _minDistEntity,
            RadiiBuffer = _radiiEntity
        });

        // Update the buffer values
        UpdateBuffers();
    }

    private void RestartSimulation()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Log for debugging
        Debug.Log($"Restarting simulation with {numberOfParticles} particles and {numberOfTypes} types");

        // Destroy all existing particles
        EntityQuery particleQuery = em.CreateEntityQuery(typeof(Particle));
        em.DestroyEntity(particleQuery);

        // Update settings with new values
        UpdateSettings();

        // Signal the particle spawner system to respawn particles
        if (em.Exists(_respawnFlagEntity))
        {
            // First ensure we have a clean state (remove if it already exists)
            if (em.HasComponent<RespawnParticles>(_respawnFlagEntity))
            {
                em.RemoveComponent<RespawnParticles>(_respawnFlagEntity);
            }
            // Then add the component to signal respawn
            em.AddComponent<RespawnParticles>(_respawnFlagEntity);
            Debug.Log("Sent respawn signal");
        }
        else
        {
            Debug.LogError("Respawn flag entity does not exist!");
            // Create a new one as fallback
            _respawnFlagEntity = em.CreateEntity();
            em.AddComponent<RespawnParticles>(_respawnFlagEntity);
        }

        // Update our tracking variables
        _prevParticleCount = numberOfParticles;
        _prevTypeCount = numberOfTypes;
    }

    private void UpdateBuffers()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get and clear the buffers
        var forces = em.GetBuffer<FloatBuffer>(_forcesEntity);
        var minDists = em.GetBuffer<FloatBuffer>(_minDistEntity);
        var radii = em.GetBuffer<FloatBuffer>(_radiiEntity);

        forces.Clear();
        minDists.Clear();
        radii.Clear();

        // Refill with new random values based on current ranges
        int size = numberOfTypes * numberOfTypes;
        for (int i = 0; i < size; i++)
        {
            float force = UnityEngine.Random.Range(_forcesRange.x, _forcesRange.y);
            float minDist = UnityEngine.Random.Range(_minDistancesRange.x, _minDistancesRange.y);
            float radius = UnityEngine.Random.Range(_radiiRange.x, _radiiRange.y);

            forces.Add(new FloatBuffer { Value = force });
            minDists.Add(new FloatBuffer { Value = minDist });
            radii.Add(new FloatBuffer { Value = radius });
        }
    }

    // Hook into Unity's inspector validation to detect changes
    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        // Check if particles count or types changed (requires simulation restart)
        if (numberOfParticles != _prevParticleCount || numberOfTypes != _prevTypeCount)
        {
            _needsRespawn = true;
        }

        _settingsDirty = true;
    }

    // Public method to manually update settings (can be called from UI)
    [Button("Update Simulation")]
    public void RefreshSettings()
    {
        _settingsDirty = true;
    }

    // Public method to manually restart the simulation
    [Button("Restart Simulation")]
    public void ForceRestart()
    {
        _needsRespawn = true;
        _settingsDirty = true;
    }

    public void ForcesRangeChanged(float lowerRange, float upperRange)
    {
        _forcesRange = new Vector2(lowerRange, upperRange);
        _settingsDirty=true;
    }

    public void MinDistancesRangeChanged(float lowerRange, float upperRange)
    {
        _minDistancesRange = new Vector2(lowerRange, upperRange);
        _settingsDirty=true;
    }
    public void RadiiRangeChanged(float lowerRange, float upperRange)
    {
        _radiiRange = new Vector2(lowerRange, upperRange);
        _settingsDirty=true;
    }

    public void NumbParticlesChanged(float number)
    {
        numberOfParticles = (int)number;
        ForceRestart();
    }

    public void NumbTypesChanged(float number)
    {
        numberOfTypes = (int)number;
        ForceRestart();
    }

    public void RepulsionEffectorChanged(float number)
    {
        repulsion = number;
        _settingsDirty = true;
    }

    public void DampeningChanged(float number)
    {
        dampening = number;
        _settingsDirty = true;
    }

    public void FrictionChanged(float number)
    {
        friction = number;
        _settingsDirty = true;
    }

    public void TimeScaleChanged(float number)
    {
        _timeScale = number;
    }
    public void ScreenSpaceChanged(float scale)
    {
        screenSpace = new Vector2(startScreenSpaceX * scale, startScreenSpaceY * scale);
        _settingsDirty=true;
    }
}