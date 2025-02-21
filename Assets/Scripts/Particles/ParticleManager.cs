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

         ////////////////////////////////////////////////////////////////
        [BoxGroup("Simulation Configuration")] [OnValueChanged("Restart")] public Vector2 ScreenSpace = new Vector2(32, 18);
        [BoxGroup("Simulation Configuration")] [HideInInspector] public Vector2 HalfScreenSpace;

        [BoxGroup("Simulation Configuration")] [OnValueChanged("Restart")] [UnityEngine.Range(1, 32)] public int NumberOfTypes = 5;
        [BoxGroup("Simulation Configuration")] [OnValueChanged("Restart")] [UnityEngine.Range(1, 9999)] public int NumberOfParticles = 500;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////
        [BoxGroup("Particle Properties")] [OnValueChanged("OnForcesRangeChanged")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] public Vector2 ForcesRange = new Vector2(0.3f, 1f);
        [BoxGroup("Particle Properties")] [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] public Vector2 MinDistancesRange = new Vector2(1f, 3f);
        [BoxGroup("Particle Properties")] [OnValueChanged("Initialize")] [MinMaxSlider(0.0f, 18.0f)] [SerializeField] public Vector2 RadiiRange = new Vector2(3f, 5f);
        [BoxGroup("Particle Properties")] [UnityEngine.Range(-5, 5)] [SerializeField] public float RepulsionEffector = -3f;

        [BoxGroup("Particle Properties")] [UnityEngine.Range(0, 1)] [SerializeField] public float Dampening = 0.05f; // Scale this down if particles are too jumpy
        [BoxGroup("Particle Properties")] [UnityEngine.Range(0, 2)] [SerializeField] public float Friction = 0.85f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////
        [BoxGroup("Unity Settings")] [OnValueChanged("ChangeTimescale")] [UnityEngine.Range(0, 5)] [SerializeField] public float _timeScale = 1f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        public int MinPopulation = 50;
        public int NumFood = 200; // starting amount of food
        public float FoodRange = 0.1f; // distance to collect food
        public int FoodEnergy = 100; // energy from food
        public int ReproductionEnergy = 1000;
        public int StartingEnergy = 200;
        public int MaxFood = 200;

        private int _runningClusterCount = 0;

        private GameObject _foodParent;

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

            _foodParent = new GameObject("FoodParent");
        }

        private void Start()
        {
            Initialize();
            //SpawnParticles();

            InvokeRepeating("SpawnFood", 0, 5);
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

            Die();  // cells die if they run out of energy
            Eat();  // cells collect nearby food
            ForceCopy();  // if the pop is below minPop add cells
            Reproduce();  // cells with lots of energy reproduce
        }

        private void SpawnFood()
        {
            if(Food.Count < MaxFood)
            {
                Food.Add(CreateFoodParticle());
            }
        }

        private Particle CreateFoodParticle()
        {
            Vector2 pos = GetRandomPointOnScreen();

            Particle temp = Instantiate(ParticlePrefab, pos, Quaternion.identity, _foodParent.transform).GetComponent<Particle>();
            temp.gameObject.name = "Food Particle";

            temp.Position = pos;
            temp.Type = 0;
            temp.ParentCluster = null; // Food should not have a parent cluster

            temp.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(0, 1, 1); // color based on type

            return temp;
        }

        private Cluster CreateCluster()
        {
            Vector2 pos = GetRandomPointOnScreen();

            Cluster temp = Instantiate(ClusterPrefab).GetComponent<Cluster>(); // make a new cell at a random location
            temp.Initialize(pos.x, pos.y);
            temp.Id = _runningClusterCount++;

            temp.gameObject.name = "Cluster";

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

        void Die() {
            for (int i = Clusters.Count - 1; i >= 0; i--)
            { // remove dead (energyless cells)
                Cluster c = Clusters[i];
                if (c.Energy <= 0)
                {
                    //convertToFood(c);
                    Clusters.RemoveAt(i);  // could convert to food instead
                    Destroy(c.gameObject);
                }
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
                    temp.gameObject.name = "Cluster (Reproduced)";

                    Debug.Log("Cluster Reproduced");

                    temp.CopyCell(c); // copy the parent cell's 'DNA'

                    c.Energy -= StartingEnergy;  // parent cell loses energy (child cell recieves it) 

                    temp.MutateCell(); // mutate the child cell

                    Clusters.Add(temp); // Add the child cell to the population
                }
            }
        }

        // If population is below minPopulation add cells by copying and mutating
        // randomly selected existing cells.
        // Note: if the population all dies simultanious the program will crash - extinction!
        void ForceCopy()
        {
            if (Clusters.Count > 0 && Clusters.Count < MinPopulation)
            {
                Cluster temp = CreateCluster();

                if(Clusters.Count > 0) { // As long as there are at least some cells, copy and mutate a random one
                    int parent = UnityEngine.Random.Range(0, Clusters.Count);
                    Cluster parentCell = Clusters[parent];
                    temp.CopyCell(parentCell);
                    temp.MutateCell();
                }
                
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
                                Particle temp = Food[i];
                                Food.RemoveAt(i);
                                Destroy(temp.gameObject);

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
            //SpawnParticles();
        }

        [Button("Reset Values", EButtonEnableMode.Playmode)]
        private void Initialize()
        {
            // This is a safety precaution as we are using the OnValueChanged, and that calls even when not in play mode.
            if (!Application.isPlaying) return;

            // Caching this to reduce repeated calculations
            HalfScreenSpace = ScreenSpace * 0.5f;

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