using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace NaughtyAttributes
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        public static List<Particle> Particles { get; private set; } = new List<Particle>();

        public static float[,] Forces;
        public static float[,] MinDistances;
        public static float[,] Radii;
        public static bool IsFinishedSpawning { get; set; } = false;


        private static Vector2 ScreenSpace = new Vector2(16, 9);
        public static float ScreenWidth => ScreenSpace.x;
        public static float ScreenHeight => ScreenSpace.y;

        public int NumTypes = 5;


        [SerializeField] private GameObject _particlePrefab;
        [SerializeField] private int _numberOfParticles = 100;

        private float _spawnRadius = 10f;

        [HorizontalLine(color: EColor.Red)]

        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 60.0f)] [SerializeField] private Vector2 _forcesRange;
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 60.0f)] [SerializeField] private Vector2 _minDistancesRange;
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 60.0f)] [SerializeField] private Vector2 _radiiRange;

        [UnityEngine.Range(-5, 5)] [SerializeField] public float RepulsionEffector;

        private void Awake()
        {
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
        }

        private void Start()
        {
            Initialize();
            SpawnParticles();
        }

        private void Initialize()
        {
            Forces = new float[NumTypes, NumTypes];
            MinDistances = new float[NumTypes, NumTypes];
            Radii = new float[NumTypes, NumTypes];
            for (int i = 0; i < NumTypes; i++)
            {
                for (int j = 0; i < NumTypes; i++)
                {
                    // Forces
                    Forces[i, j] = Random.Range(_forcesRange.x, _forcesRange.y);
                    if (Random.Range(0, 1) > 0.5f)
                    {
                        Forces[i, j] *= -1;
                    }

                    // Minimum distances
                    MinDistances[i, j] = Random.Range(_minDistancesRange.x, _minDistancesRange.y);
                    Radii[i, j] = Random.Range(_radiiRange.x, _radiiRange.y);
                }
            }
        }

        private void SpawnParticles()
        {
            for (int i = 0; i < _numberOfParticles; i++)
            {
                // Get a random position around the center 0,0
                Vector3 randomPos = new Vector3(Random.Range(-_spawnRadius, _spawnRadius), Random.Range(-_spawnRadius, _spawnRadius), 0);
                GameObject particle = Instantiate(_particlePrefab, randomPos, Quaternion.identity);
                Particle particleScript = particle.GetComponent<Particle>();
                if (particleScript != null)
                {
                    ParticleManager.Particles.Add(particle.GetComponent<Particle>());
                    particleScript.Type = Random.Range(0, NumTypes);
                    particleScript.Id = i;
                    Color color = Color.HSVToRGB((float)particleScript.Type / NumTypes, 1, 1);
                    particle.GetComponent<SpriteRenderer>().color = color;
                }
            }
            IsFinishedSpawning = true;
        }
    }
}