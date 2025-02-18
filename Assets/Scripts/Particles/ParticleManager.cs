using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.ParticleSystem;

namespace NaughtyAttributes
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        public static List<ParticleData> Particles { get; private set; } = new List<ParticleData>();

        public static float[,] Forces;
        public static float[,] MinDistances;
        public static float[,] Radii;
        public static bool IsFinishedSpawning { get; set; } = false;

        private GameObject _particleParentObj;
        private ParticleJobManager jobManager;

        [SerializeField] private GameObject _particlePrefab;
        public ParticleSystem ParticleSystem;

        [Header("Simulation Configuration")] ////////////////////////////////////////////////////////////////
        [OnValueChanged("Restart")] public Vector2 ScreenSpace = new Vector2(32, 18);
        [HideInInspector] public Vector2 HalfScreenSpace;

        [OnValueChanged("Restart")] [UnityEngine.Range(1, 32)] public int NumberOfTypes = 5;
        [OnValueChanged("Restart")] [UnityEngine.Range(1, 9999)] public int NumberOfParticles = 500;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] private Vector2 _forcesRange = new Vector2(0.3f, 1f);
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] private Vector2 _minDistancesRange = new Vector2(1f, 3f);
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] private Vector2 _radiiRange = new Vector2(3f, 5f);
        [UnityEngine.Range(-5, 5)] [SerializeField] public float RepulsionEffector = -3f;

        [UnityEngine.Range(0, 1)] [SerializeField] public float Dampening = 0.05f; // Scale this down if particles are too jumpy
        [UnityEngine.Range(0, 2)] [SerializeField] public float Friction = 0.85f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Unity Settings")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("ChangeTimescale")] [UnityEngine.Range(0, 5)] [SerializeField] public float _timeScale = 1f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        private void ChangeTimescale()
        {
            Time.timeScale = _timeScale;
        }

        private void Awake()
        {
            // Singleton initialization
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            // Make sure the particle prefab is assigned
            if (_particlePrefab == null)
            {
                Debug.LogError("Particle prefab is not assigned in the inspector.");
                return;
            }

            // Enable running in background so the game does not need to be focused on to run
            Application.runInBackground = true;

            // Cache the job manager
            jobManager = GetComponent<ParticleJobManager>();
        }

        private void Start()
        {
            Initialize();
            SpawnParticles();
        }

        private void Update()
        {
            // Restart the simulation if the "R" key is pressed
            if(Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ParticleSystem.Emit(2);
            }
        }

        [Button("Restart Simulation", EButtonEnableMode.Playmode)]
        private void Restart()
        {
            // Restarts the simulation by first clearing all particles and then restarting the process
            ClearParticles();
            Initialize();
            SpawnParticles();
        }

        [Button("Reset Values", EButtonEnableMode.Playmode)]
        private void Initialize()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            // Caching this to reduce calculations
            HalfScreenSpace = ScreenSpace * 0.5f;

            Forces = new float[NumberOfTypes, NumberOfTypes];
            MinDistances = new float[NumberOfTypes, NumberOfTypes];
            Radii = new float[NumberOfTypes, NumberOfTypes];
            for (int i = 0; i < NumberOfTypes; i++)
            {
                for (int j = 0; j < NumberOfTypes; j++)
                {
                    // Forces
                    Forces[i, j] = Random.Range(_forcesRange.x, _forcesRange.y);
                    if (Random.Range(0f, 1f) > 0.5f)
                    {
                        Forces[i, j] *= -1;
                    }

                    // Minimum distances
                    MinDistances[i, j] = Random.Range(_minDistancesRange.x, _minDistancesRange.y);
                    
                    // Radii
                    Radii[i, j] = Random.Range(_radiiRange.x, _radiiRange.y);
                }
            }

            // Update the tables in the job manager
            jobManager.AssignTables(Forces, MinDistances, Radii);
        }

        private void SpawnParticles()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            ParticleSystem.MainModule main = ParticleSystem.main;
            main.maxParticles = NumberOfParticles;

            ParticleSystem.Emit(NumberOfParticles);

            var individualParticles = new ParticleSystem.Particle[ParticleManager.Instance.NumberOfParticles];

            int activeCount = ParticleManager.Instance.ParticleSystem.GetParticles(individualParticles);
            if(NumberOfParticles != activeCount)
            {
                Debug.LogError("Error spawning particles!");
                return;
            }

            for (int i = 0; i < NumberOfParticles; i++)
            {
                // Find a random position around (0, 0) using _spawnRadius
                Vector3 randomPos = new Vector3(Random.Range(-HalfScreenSpace.x, HalfScreenSpace.x), Random.Range(-HalfScreenSpace.y, HalfScreenSpace.y), 0);

                // Create the particle, rename it, and put it under the parent GameObject
                //GameObject particle = Instantiate(_particlePrefab, randomPos, Quaternion.identity);
                //particle.name = "Particle " + i;
                //particle.transform.parent = _particleParentObj.transform;

                ParticleData data = new ParticleData()
                {
                    Id = i,
                    Type = Random.Range(0, NumberOfTypes),
                };

                ParticleManager.Particles.Add(data);
                Color color = Color.HSVToRGB((float)data.Type / NumberOfTypes, 1, 1);

                individualParticles[i].startColor = color;
                individualParticles[i].position = randomPos;
            }
            
            IsFinishedSpawning = true;
            jobManager.Initialize();
        }

        private void ClearParticles()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;
            
            IsFinishedSpawning = false;

            ParticleSystem.Clear();
        }
    }

    public struct ParticleData
    {
        public int Id;
        public int Type;
    }
}