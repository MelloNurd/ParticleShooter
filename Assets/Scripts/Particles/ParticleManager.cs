using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.ParticleSystem;

namespace NaughtyAttributes
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        public static List<Particle> Particles { get; private set; } = new List<Particle>();
        public static List<Particle> Food { get; private set; } = new List<Particle>();
        public static List<Cluster> Clusters { get; private set; } = new List<Cluster>();

        public GameObject ParticlePrefab;
        public GameObject ClusterPrefab;
        
        public static float[,] Forces;
        public static float[,] MinDistances;
        public static float[,] Radii;
        public static bool IsFinishedSpawning { get; set; } = false;

        private GameObject _particleParentObj;

        public static event Action<Vector2> ForcesRangeChanged;

        [Header("Simulation Configuration")] ////////////////////////////////////////////////////////////////
        [OnValueChanged("Restart")] public Vector2 ScreenSpace = new Vector2(32, 18);
        [HideInInspector] public Vector2 HalfScreenSpace;

        [OnValueChanged("Restart")] [UnityEngine.Range(1, 32)] public int NumberOfTypes = 5;
        [OnValueChanged("Restart")] [UnityEngine.Range(1, 9999)] public int NumberOfParticles = 500;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("OnForcesRangeChanged")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] public Vector2 ForcesRange = new Vector2(0.3f, 1f);
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] public Vector2 MinDistancesRange = new Vector2(1f, 3f);
        [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] public Vector2 RadiiRange = new Vector2(3f, 5f);
        [UnityEngine.Range(-5, 5)] [SerializeField] public float RepulsionEffector = -3f;

        [UnityEngine.Range(0, 1)] [SerializeField] public float Dampening = 0.05f; // Scale this down if particles are too jumpy
        [UnityEngine.Range(0, 2)] [SerializeField] public float Friction = 0.85f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Unity Settings")] /////////////////////////////////////////////////////////////////////
        [OnValueChanged("ChangeTimescale")] [UnityEngine.Range(0, 5)] [SerializeField] public float _timeScale = 1f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        public int MinPopulation = 15;
        public int NumFood = 200; // starting amount of food
        public int FoodRange = 5; // distance to collect food
        public int FoodEnergy = 100; // energy from food
        public int ReproductionEnergy = 1000;
        public int StartingEnergy = 400;

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
            if (ParticlePrefab == null || ClusterPrefab == null)
            {
                Debug.LogError("One or more prefab is not assigned in the inspector!");
                return;
            }

            // Enable running in background so the game does not need to be focused on to run
            Application.runInBackground = true;
        }

        private void Start()
        {
            Initialize();
            //SpawnParticles();
        }

        private void Update()
        {
            // Inputs
            if(Input.GetKeyDown(KeyCode.R)) // Restart the simulation if the "R" key is pressed
            {
                Restart();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                SwapForces();
            }

            // Simulation
            foreach(Cluster colony in Clusters)
            {
                colony.Tick();
            }

            for (int i = Clusters.Count - 1; i >= 0; i--)
            { // remove dead (energyless cells)
                Cluster c = Clusters[i];
                if (c.Energy <= 0)
                {
                    //convertToFood(c);
                    Clusters.RemoveAt(i);  // could convert to food instead
                }
            }

            Eat();  // cells collect nearby food
            Replace();  // if the pop is below minPop add cells
            Reproduce();  // cells with lots of energy reproduce

            if(Time.frameCount % 5 == 0)
            {
                Food.Add(CreateFoodParticle());
            }
        }

        private Particle CreateFoodParticle()
        {
            Vector2 pos = GetRandomPointOnScreen();

            Particle temp = Instantiate(ParticlePrefab, pos, Quaternion.identity).GetComponent<Particle>();
            temp.Type = 0;

            return temp;
        }

        private Cluster CreateCluster()
        {
            Vector2 pos = GetRandomPointOnScreen();

            Cluster temp = Instantiate(ClusterPrefab).GetComponent<Cluster>(); // make a new cell at a random location
            temp.Initialize(pos.x, pos.y);

            return temp;
        }

        // for dead cells
        void ConvertToFood(Cluster c)
        {
            foreach (Particle p in c.Swarm)
            {
                Food.Add(CreateFoodParticle());
            }
        }

        void Reproduce()
        {
            Cluster c;
            for (int i = Clusters.Count - 1; i >= 0; i--)
            {
                c = Clusters[i];
                if (c.Energy > ReproductionEnergy)
                { // if a cell has enough energy 
                    Cluster temp = CreateCluster();

                    temp.CopyCell(c); // copy the parent cell's 'DNA'

                    c.Energy -= StartingEnergy;  // parent cell loses energy (daughter cell recieves it) 

                    temp.MutateCell(); // mutate the daughter cell
                }
            }
        }

        // If population is below minPopulation add cells by copying and mutating
        // randomly selected existing cells.
        // Note: if the population all dies simultanious the program will crash - extinction!
        void Replace()
        {
            Debug.Log("COUNT: " + Clusters.Count);
            if (Clusters.Count < MinPopulation)
            {
                int parent = UnityEngine.Random.Range(0, Clusters.Count);

                Cluster temp = CreateCluster();

                Cluster parentCell = Clusters[parent];

                temp.CopyCell(parentCell);

                temp.MutateCell();

                Clusters.Add(temp);
            }
        }

        void Eat()
        {
            float dis;
            Vector3 vector = Vector3.zero;
            foreach (Cluster c in Clusters)
            {  // for every cell
                foreach (Particle p in c.Swarm)
                {  // for every particle in every cell
                    if (p.Type == 1)
                    { // 1 is the eating type of paricle
                        for (int i = Food.Count - 1; i >= 0; i--)
                        {  // for every food particle - yes this gets slow
                            Particle f = Food[i];
                            vector = f.Position - p.Position;

                            if (vector.x > HalfScreenSpace.x) { vector.x -= ScreenSpace.x; }
                            if (vector.x < -HalfScreenSpace.x) { vector.x += ScreenSpace.x; }
                            if (vector.y > HalfScreenSpace.y) { vector.y -= ScreenSpace.y; }
                            if (vector.y < -HalfScreenSpace.y) { vector.y += ScreenSpace.y; }

                            dis = vector.magnitude;
                            if (dis < FoodRange)
                            {
                                c.Energy += FoodEnergy; // gain 100 energy for eating food 
                                Food.RemoveAt(i);
                            }
                        }
                    }
                }
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

            //Forces = new float[NumberOfTypes, NumberOfTypes];
            //MinDistances = new float[NumberOfTypes, NumberOfTypes];
            //Radii = new float[NumberOfTypes, NumberOfTypes];
            //for (int i = 0; i < NumberOfTypes; i++)
            //{
            //    for (int j = 0; j < NumberOfTypes; j++)
            //    {
            //        // Forces
            //        Forces[i, j] = UnityEngine.Random.Range(ForcesRange.x, ForcesRange.y);
            //        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
            //        {
            //            Forces[i, j] *= -1;
            //        }

            //        // Minimum distances
            //        MinDistances[i, j] = UnityEngine.Random.Range(MinDistancesRange.x, MinDistancesRange.y);
                    
            //        // Radii
            //        Radii[i, j] = UnityEngine.Random.Range(RadiiRange.x, RadiiRange.y);
            //    }
            //}

            for (int i = 0; i < MinPopulation; i++)
            {
                Clusters.Add(CreateCluster());
            }

            for (int i = 0; i < NumFood; i++)
            {
                Food.Add(CreateFoodParticle());
            }
        }

        private void OnForcesRangeChanged()
        {
            Initialize();
            ForcesRangeChanged?.Invoke(ForcesRange);
        }

        private void SpawnParticles()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            _particleParentObj = new GameObject("ParticleParent");
            for (int i = 0; i < NumberOfParticles; i++)
            {
                // Find a random position around (0, 0) using _spawnRadius
                Vector3 randomPos = GetRandomPointOnScreen();
                
                // Create the particle, rename it, and put it under the parent GameObject
                GameObject particle = Instantiate(ParticlePrefab, randomPos, Quaternion.identity);
                particle.name = "Particle " + i;
                particle.transform.parent = _particleParentObj.transform;

                Particle particleScript = particle.GetComponent<Particle>();
                if (particleScript != null)
                {
                    particleScript.Position = particle.transform.position;
                    ParticleManager.Particles.Add(particle.GetComponent<Particle>());
                    particleScript.Type = UnityEngine.Random.Range(0, NumberOfTypes);
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

        public Vector3 GetRandomPointOnScreen()
        {
            return new Vector3(UnityEngine.Random.Range(-HalfScreenSpace.x, HalfScreenSpace.x), UnityEngine.Random.Range(-HalfScreenSpace.y, HalfScreenSpace.y), 0);
        }
    }

}