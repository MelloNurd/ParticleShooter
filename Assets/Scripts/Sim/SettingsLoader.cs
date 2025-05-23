using System;
using System.IO;
using NaughtyAttributes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using static Unity.Entities.SystemBaseDelegates;

public struct EntitySimSettings : IComponentData
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
    [OnValueChanged("RefreshSettings")] public Vector2 screenSpace = new Vector2(32, 18);
    [OnValueChanged("RefreshSettings")][UnityEngine.Range(1, 9999)] public int numberOfParticles = 1000;
    [OnValueChanged("RefreshSettings")][UnityEngine.Range(1, 32)] public int numberOfTypes = 5;

    [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
    [OnValueChanged("RefreshSettings")][MinMaxSlider(0.0f, 18.0f)] public Vector2 forcesRange = new Vector2(0.3f, 1f);
    [OnValueChanged("RefreshSettings")][MinMaxSlider(0.0f, 18.0f)] public Vector2 minDistancesRange = new Vector2(1f, 3f);
    [OnValueChanged("RefreshSettings")][MinMaxSlider(0.0f, 18.0f)] public Vector2 radiiRange = new Vector2(3f, 5f);

    [Space(10)]
    [OnValueChanged("RefreshSettings")][UnityEngine.Range(-5, 5)] public float repulsion = -5f;
    [OnValueChanged("RefreshSettings")][UnityEngine.Range(0, 1)] public float friction = 0.95f;
    [OnValueChanged("RefreshSettings")][UnityEngine.Range(0, 1)] public float dampening = 0.5f;

    [Header("Unity Settings")] /////////////////////////////////////////////////////////////////////
    [OnValueChanged("RefreshSettings")][UnityEngine.Range(0, 5)] public float timeScale = 1f;

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
    public static UnityEvent SettingsChanged = new();

    private bool _systemReady = false;

    private void OnEnable() => _systemReady = false;

    private Vector2 screenSize;

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

        screenSize = new Vector2(Screen.width, Screen.height);
        Debug.Log("In awake screensize");
        screenSpace = new Vector2(Screen.width * 0.0333f, Screen.height * 0.0333f);
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        _systemReady = true;

        InitializeSettings();
        _prevParticleCount = numberOfParticles;
        _prevTypeCount = numberOfTypes;
    }

    void Update()
    {
        // Update timeScale globally
        Time.timeScale = timeScale;

        Vector2 currentScreen = new Vector2(Screen.width, Screen.height);
        if (currentScreen != screenSize)
        {
            Vector2 oldScreenSpace = screenSpace;
            screenSize = currentScreen;
            //screenSpace = new Vector2(Screen.width * 0.0333f, Screen.height * 0.0333f);
            CameraScaler.Instance.ScreenShapeChanged(oldScreenSpace, new Vector2(Screen.width * 0.0333f, Screen.height * 0.0333f));
            _settingsDirty = true;
        }

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
            SettingsChanged?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            SaverLoader.SaveData(GetSettings());
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            var Test = SaverLoader.LoadData(Application.persistentDataPath + "/settings.json");
            Test.PrintAll();
            SetSettings(Test);
        }
    }

    public SimSettings GetSettings()
    {
        SimSettings settings = new SimSettings
        {
            ScreenSpace = screenSpace,
            NumberOfParticles = numberOfParticles,
            NumberOfTypes = numberOfTypes,
            forces = forcesRange,
            minDistances = minDistancesRange,
            radii = radiiRange,
            RepulsionEffector = repulsion,
            Friction = friction,
            Dampening = dampening,
            TimeScale = timeScale
        };
        return settings;
    }

    public void SetSettings(SimSettings settings)
    {
        Debug.Log("In Set Settings");
        screenSpace = settings.ScreenSpace;
        numberOfParticles = settings.NumberOfParticles;
        numberOfTypes = settings.NumberOfTypes;
        forcesRange = settings.forces;
        minDistancesRange = settings.minDistances;
        radiiRange = settings.radii;
        repulsion = settings.RepulsionEffector;
        friction = settings.Friction;
        dampening = settings.Dampening;
        timeScale = settings.TimeScale;

        ForceRestart();
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
        em.AddComponentData(_simSettingsEntity, new EntitySimSettings
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
        em.SetComponentData(_simSettingsEntity, new EntitySimSettings
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
        }
        else
        {
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
            float force = UnityEngine.Random.Range(forcesRange.x, forcesRange.y);
            float minDist = UnityEngine.Random.Range(minDistancesRange.x, minDistancesRange.y);
            float radius = UnityEngine.Random.Range(radiiRange.x, radiiRange.y);

            forces.Add(new FloatBuffer { Value = force });
            minDists.Add(new FloatBuffer { Value = minDist });
            radii.Add(new FloatBuffer { Value = radius });
        }
    }

    // Public method to manually update settings (can be called from UI)
    [Button("Update Simulation")]
    public void RefreshSettings()
    {
        if (!_systemReady) return;

        _settingsDirty = true;

        if(_prevParticleCount != numberOfParticles || _prevTypeCount != numberOfTypes)
        {
            _needsRespawn = true; // will cause a restart
        }
    }

    // Public method to manually restart the simulation
    [Button("Restart Simulation")]
    public void ForceRestart()
    {
        if (!_systemReady) return;

        _needsRespawn = true;
        RefreshSettings();
    }
}