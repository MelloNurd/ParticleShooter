using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NaughtyAttributes
{
    public class SimParticleManager : MonoBehaviour
    {
        public static SimParticleManager Instance { get; private set; }

        public static List<SimParticle> Particles { get; private set; } = new List<SimParticle>();

        [SerializeField] public GameObject ParticlePrefab;

        public static float[,] Forces;
        public static float[,] MinDistances;
        public static float[,] Radii;
        public static bool IsFinishedSpawning { get; set; } = false;

        private GameObject _particleParentObj;
        private SimParticleJobManager jobManager;

        [Header("Simulation Configuration")] ////////////////////////////////////////////////////////////////
        [OnValueChanged("Restart")] public Vector2 ScreenSpace = new Vector2(32, 18);
        [HideInInspector] public Vector2 HalfScreenSpace;

        [OnValueChanged("Restart")][UnityEngine.Range(1, 32)] public int NumberOfTypes = 5;
        [OnValueChanged("Restart")][UnityEngine.Range(1, 9999)] public int NumberOfParticles = 500;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("Initialize")][MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _forcesRange = new Vector2(0.3f, 1f);
        [OnValueChanged("Initialize")][MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _minDistancesRange = new Vector2(1f, 3f);
        [OnValueChanged("Initialize")][MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _radiiRange = new Vector2(3f, 5f);
        [UnityEngine.Range(-5, 5)][SerializeField] public float RepulsionEffector = -3f;

        [UnityEngine.Range(0, 1)][SerializeField] public float Dampening = 0.05f; // Scale this down if particles are too jumpy
        [UnityEngine.Range(0, 2)][SerializeField] public float Friction = 0.85f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Unity Settings")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("ChangeTimescale")][UnityEngine.Range(0, 5)][SerializeField] public float _timeScale = 1f;
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
            if (ParticlePrefab == null)
            {
                Debug.LogError("Particle prefab is not assigned in the inspector.");
                return;
            }

            // Enable running in background so the game does not need to be focused on to run
            Application.runInBackground = true;

            // Cache the job manager
            jobManager = GetComponent<SimParticleJobManager>();
        }

        private void Start()
        {
            Initialize();
            SpawnParticles();
        }

        private void Update()
        {
            // Restart the simulation if the "R" key is pressed
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                SwapForces();
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

            _particleParentObj = new GameObject("ParticleParent");
            for (int i = 0; i < NumberOfParticles; i++)
            {
                // Find a random position around (0, 0) using _spawnRadius
                Vector3 randomPos = new Vector3(Random.Range(-HalfScreenSpace.x, HalfScreenSpace.x), Random.Range(-HalfScreenSpace.y, HalfScreenSpace.y), 0);

                // Create the particle, rename it, and put it under the parent GameObject
                GameObject particle = Instantiate(ParticlePrefab, randomPos, Quaternion.identity);
                particle.name = "Particle " + i;
                particle.transform.parent = _particleParentObj.transform;

                SimParticle particleScript = particle.GetComponent<SimParticle>();
                if (particleScript != null)
                {
                    SimParticleManager.Particles.Add(particle.GetComponent<SimParticle>());
                    particleScript.Type = Random.Range(0, NumberOfTypes);
                    particleScript.Id = i;
                    Color color = Color.HSVToRGB((float)particleScript.Type / NumberOfTypes, 1, 1);
                    particle.GetComponent<SpriteRenderer>().color = color;
                }
            }

            IsFinishedSpawning = true;
            jobManager.Initialize();
        }

        private void ClearParticles()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            IsFinishedSpawning = false;

            GameObject temp;
            for (int i = Particles.Count - 1; i >= 0; i--) // Unsure if necessary, but traversing backwards through the list just in case
            {
                temp = Particles[i].gameObject;
                Particles.Remove(Particles[i]);
                Destroy(temp);
            }

            Destroy(_particleParentObj);
        }
        private void SwapForces()
        {
            // Swap the forces between particle types
            for (int i = 0; i < NumberOfTypes; i++)
            {
                for (int j = i + 1; j < NumberOfTypes; j++)
                {
                    float temp = Forces[i, j];
                    Forces[i, j] = Forces[j, i];
                    Forces[j, i] = temp;
                }
            }
            Debug.Log("Forces swapped between particle types.");
        }
    }
}