using System.Collections.Generic;
using UnityEngine;

namespace NaughtyAttributes
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        public Player player;

        public float PlayerAttractionStrength = 5f;

        public List<Cluster> Clusters = new List<Cluster>();

        public GameObject ParticlePrefab;
        public GameObject ClusterPrefab;

        [BoxGroup("Simulation Configuration")]
        [OnValueChanged("Restart")]
        public Vector2 ScreenSpace = new Vector2(32, 18);

        [BoxGroup("Simulation Configuration")]
        [HideInInspector]
        public Vector2 HalfScreenSpace;

        [BoxGroup("Simulation Configuration")]
        [OnValueChanged("Restart")]
        [Range(1, 32)]
        public int NumberOfTypes = 5;

        [BoxGroup("Particle Properties")]
        [Range(-5, 5)]
        public float RepulsionEffector = -3f;

        [BoxGroup("Particle Properties")]
        [Range(0, 1)]
        public float Dampening = 0.1f;

        [BoxGroup("Particle Properties")]
        [Range(0, 2)]
        public float Friction = 0.9f;

        [BoxGroup("Unity Settings")]
        [OnValueChanged("ChangeTimescale")]
        [Range(0, 5)]
        public float _timeScale = 1f;

        [BoxGroup("Force Parameters")]
        [MinMaxSlider(-5f, 5f)]
        public Vector2 InternalForceRange = new Vector2(2f, 5f);

        [BoxGroup("Force Parameters")]
        [MinMaxSlider(-5f, 5f)]
        public Vector2 ExternalForceRange = new Vector2(-5f, 5f);

        [BoxGroup("Force Parameters")]
        [MinMaxSlider(0.1f, 2f)]
        public Vector2 InternalMinDistanceRange = new Vector2(0.1f, 0.5f);

        [BoxGroup("Force Parameters")]
        [MinMaxSlider(1f, 5f)]
        public Vector2 ExternalMinDistanceRange = new Vector2(1f, 2f);

        [BoxGroup("Force Parameters")]
        [MinMaxSlider(0.5f, 5f)]
        public Vector2 InternalRadiusRange = new Vector2(0.5f, 2f);

        [BoxGroup("Force Parameters")]
        [MinMaxSlider(2f, 7f)]
        public Vector2 ExternalRadiusRange = new Vector2(2f, 7f);

        [BoxGroup("Force Parameters")]
        [Range(0f, 5f)]
        public float CohesionStrength = 2f;

        public int StartPopulation = 5;

        private int _runningClusterCount = 0;

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
            Initialize();
        }

        private void Update()
        {
            // Input handling
            if (Input.GetKeyDown(KeyCode.R)) // Restart the simulation if the "R" key is pressed
            {
                Restart();
            }

            // Check for cluster extinction
            CheckForClusterExtinction();

            // Update all clusters
            foreach (Cluster cluster in Clusters)
            {
                cluster.UpdateCluster();
            }
        }

        private void CheckForClusterExtinction()
        {
            for (int i = Clusters.Count - 1; i >= 0; i--)
            {
                Cluster cluster = Clusters[i];
                if (cluster.Swarm.Count == 0 || cluster.Swarm.Count <= cluster.numParticles * 0.1f)
                {
                    // Find the most successful cluster
                    Cluster mostSuccessfulCluster = FindMostSuccessfulCluster();

                    if (mostSuccessfulCluster != null)
                    {
                        // Spawn a mutated version of the most successful cluster
                        SpawnMutatedCluster(mostSuccessfulCluster);
                        Debug.Log($"Cluster {cluster.Id} has gone extinct! Spawning mutated cluster based on Cluster {mostSuccessfulCluster.Id}.");
                    }
                    else
                    {
                        // If no successful cluster exists, create a new random cluster
                        CreateCluster();
                        Debug.Log($"Cluster {cluster.Id} has gone extinct! No successful cluster found. Creating a new random cluster.");
                    }

                    Clusters.RemoveAt(i);
                    Destroy(cluster.gameObject);
                }
            }
        }

        private Cluster FindMostSuccessfulCluster()
        {
            Cluster mostSuccessful = null;
            int maxHits = -1;

            foreach (var cluster in Clusters)
            {
                if (cluster.HitsToPlayer > maxHits)
                {
                    maxHits = cluster.HitsToPlayer;
                    mostSuccessful = cluster;
                }
            }

            return mostSuccessful;
        }

        private void SpawnMutatedCluster(Cluster baseCluster)
        {
            Debug.Log($"Spawning new mutated cluster based on Cluster {baseCluster.Id}.");

            Vector2 pos = GetRandomPointOnScreen();
            Cluster newCluster = Instantiate(ClusterPrefab, pos, Quaternion.identity).GetComponent<Cluster>();

            // Copy the base cluster's properties
            newCluster.numParticles = baseCluster.numParticles;
            newCluster.MaxInternalRadii = baseCluster.MaxInternalRadii;
            newCluster.MaxExternalRadii = baseCluster.MaxExternalRadii;

            // Ensure _numTypes is set
            newCluster._numTypes = baseCluster._numTypes;

            // Copy and mutate the force matrices
            newCluster.InternalForces = MutateForceMatrix(baseCluster.InternalForces);
            newCluster.ExternalForces = MutateForceMatrix(baseCluster.ExternalForces);
            newCluster.InternalMins = MutateForceMatrix(baseCluster.InternalMins);
            newCluster.ExternalMins = MutateForceMatrix(baseCluster.ExternalMins);
            newCluster.InternalRadii = MutateForceMatrix(baseCluster.InternalRadii);
            newCluster.ExternalRadii = MutateForceMatrix(baseCluster.ExternalRadii);

            newCluster.Initialize(pos.x, pos.y);
            newCluster.Id = _runningClusterCount++;
            newCluster.gameObject.name = $"Cluster (Mutated from {baseCluster.Id})";

            Clusters.Add(newCluster);
        }


        private float[,] MutateForceMatrix(float[,] matrix)
        {
            int size0 = matrix.GetLength(0);
            int size1 = matrix.GetLength(1);
            float[,] mutatedMatrix = new float[size0, size1];

            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    float mutation = Random.Range(-0.1f, 0.1f); // Adjust mutation range as needed
                    mutatedMatrix[i, j] = matrix[i, j] + mutation;
                }
            }

            return mutatedMatrix;
        }

        private Cluster CreateCluster()
        {
            Debug.Log("Creating new cluster.");
            Vector2 pos = GetRandomPointOnScreen();

            Cluster newCluster = Instantiate(ClusterPrefab, pos, Quaternion.identity).GetComponent<Cluster>();
            newCluster.Initialize(pos.x, pos.y);
            newCluster.Id = _runningClusterCount++;

            newCluster.gameObject.name = "Cluster";

            Clusters.Add(newCluster);

            return newCluster;
        }

        private void OnValidate()
        {
            UpdateClusterParameters();
        }

        public void UpdateClusterParameters()
        {
            foreach (var cluster in Clusters)
            {
                cluster.UpdateForceParameters();
            }
        }


        [Button("Restart Simulation", EButtonEnableMode.Playmode)]
        private void Restart()
        {
            // Clear all clusters and restart
            ClearClusters();
            Initialize();
        }

        [Button("Reset Values", EButtonEnableMode.Playmode)]
        private void Initialize()
        {
            if (!Application.isPlaying) return;

            // Calculate half screen space
            HalfScreenSpace = ScreenSpace * 0.5f;

            // Spawn initial clusters
            for (int i = 0; i < StartPopulation; i++)
            {
                CreateCluster();
            }
        }

        private void ClearClusters()
        {
            for (int i = Clusters.Count - 1; i >= 0; i--)
            {
                Cluster cluster = Clusters[i];
                Clusters.RemoveAt(i);
                Destroy(cluster.gameObject);
            }

            _runningClusterCount = 0;
        }

        public Vector3 GetRandomPointOnScreen()
        {
            return new Vector3(
                Random.Range(-HalfScreenSpace.x, HalfScreenSpace.x),
                Random.Range(-HalfScreenSpace.y, HalfScreenSpace.y),
                0);
        }
    }
}
