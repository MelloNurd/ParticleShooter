using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace NaughtyAttributes
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        public static List<Particle> Particles { get; private set; } = new List<Particle>();

        [SerializeField] private GameObject _particlePrefab;
        
        public static float[,] Forces;
        public static float[,] MinDistances;
        public static float[,] Radii;
        public static bool IsFinishedSpawning { get; set; } = false;

        private GameObject _particleParentObj;

        [Header("Simulation Configuration")] ////////////////////////////////////////////////////////////////
        [OnValueChanged("Restart")] [SerializeField] private Vector2 ScreenSpace = new Vector2(16, 9);
        public float ScreenWidth => ScreenSpace.x;
        public float ScreenHeight => ScreenSpace.y;

        [OnValueChanged("Restart")] [UnityEngine.Range(1, 32)] public int NumberOfTypes = 5;
        [OnValueChanged("Restart")] [UnityEngine.Range(1, 9999)] public int NumberOfParticles = 500;
        [OnValueChanged("Restart")] [UnityEngine.Range(1, 99)] [SerializeField] private float _spawnRadius = 10f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 5.0f)] [SerializeField] private Vector2 _forcesRange = new Vector2(0.3f, 1f);
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 100.0f)] [SerializeField] private Vector2 _minDistancesRange = new Vector2(30f, 50f);
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 1000.0f)] [SerializeField] private Vector2 _radiiRange = new Vector2(70f, 250f);
        [UnityEngine.Range(-5, 5)] [SerializeField] public float RepulsionEffector = -3f;

        [UnityEngine.Range(0, 1)] [SerializeField] public float Dampening = 0.05f; // Scale this down if particles are too jumpy
        [UnityEngine.Range(0, 2)] [SerializeField] public float Friction = 0.85f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

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
        }

        [Button("Restart Simulation", EButtonEnableMode.Playmode)]
        private void Restart()
        {
            // Restarts the simulation by first clearing all particles and then restarting the process
            ClearParticles();
            Initialize();
            SpawnParticles();
        }

        private void Initialize()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            Forces = new float[NumberOfTypes, NumberOfTypes];
            MinDistances = new float[NumberOfTypes, NumberOfTypes];
            Radii = new float[NumberOfTypes, NumberOfTypes];
            for (int i = 0; i < NumberOfTypes; i++)
            {
                for (int j = 0; j < NumberOfTypes; j++)
                {
                    // Forces
                    Forces[i, j] = Random.Range(_forcesRange.x, _forcesRange.y);
                    if (Random.Range(0, 1) > 0.5f)
                    {
                        Forces[i, j] *= -1;
                    }

                    // Minimum distances
                    MinDistances[i, j] = Random.Range(_minDistancesRange.x, _minDistancesRange.y);
                    
                    // Radii
                    Radii[i, j] = Random.Range(_radiiRange.x, _radiiRange.y);
                }
            }
        }

        private void SpawnParticles()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            _particleParentObj = new GameObject("ParticleParent");
            for (int i = 0; i < NumberOfParticles; i++)
            {
                // Find a random position around (0, 0) using _spawnRadius
                Vector3 randomPos = new Vector3(Random.Range(-_spawnRadius, _spawnRadius), Random.Range(-_spawnRadius, _spawnRadius), 0);
                
                // Create the particle, rename it, and put it under the parent GameObject
                GameObject particle = Instantiate(_particlePrefab, randomPos, Quaternion.identity);
                particle.name = "Particle " + i;
                particle.transform.parent = _particleParentObj.transform;

                Particle particleScript = particle.GetComponent<Particle>();
                if (particleScript != null)
                {
                    ParticleManager.Particles.Add(particle.GetComponent<Particle>());
                    particleScript.Type = Random.Range(0, NumberOfTypes);
                    particleScript.Id = i;
                    Color color = Color.HSVToRGB((float)particleScript.Type / NumberOfTypes, 1, 1);
                    particle.GetComponent<SpriteRenderer>().color = color;
                }
            }
            IsFinishedSpawning = true;
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
    }
}