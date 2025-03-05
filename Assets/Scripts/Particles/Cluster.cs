using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace NaughtyAttributes
{
    public class Cluster : MonoBehaviour
    {
        public List<Particle> Swarm { get; set; } = new List<Particle>();
        public int Id { get; set; }

        public float[,] InternalForces;
        public float[,] ExternalForces;
        public float[,] InternalMins;
        public float[,] ExternalMins;
        public float[,] InternalRadii;
        public float[,] ExternalRadii;

        public float MaxInternalRadii { get; set; }
        public float MaxExternalRadii { get; set; }
        public Vector3 Center;

        public int HitsToPlayer { get; set; } = 0;

        public int numParticles = 40;

        private GameObject _particlePrefab;
        public int _numTypes;

        public float ProximityScore { get; private set; }
        public int TotalParticlesDestroyed { get; private set; }

        private Player _player;

        private void Awake()
        {
            // Initialize particle prefab and number of types from ParticleManager
            _particlePrefab = ParticleManager.Instance.ParticlePrefab;
            _numTypes = ParticleManager.Instance.NumberOfTypes;
            _player = FindFirstObjectByType<Player>();
        }

        public void Initialize(float x, float y)
        {
            // Initialize force matrices and generate new particles
            InitializeForceMatrices();
            GenerateNew(x, y);
        }

        private void InitializeForceMatrices()
        {
            // Initialize the force matrices based on the number of types
            InternalForces = new float[_numTypes, _numTypes];
            ExternalForces = new float[_numTypes, _numTypes];
            InternalMins = new float[_numTypes, _numTypes];
            ExternalMins = new float[_numTypes, _numTypes];
            InternalRadii = new float[_numTypes, _numTypes];
            ExternalRadii = new float[_numTypes, _numTypes];

            // Initialize with default or random values
            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] = Random.Range(ParticleManager.Instance.InternalForceRange.x, ParticleManager.Instance.InternalForceRange.y);
                    ExternalForces[i, j] = Random.Range(ParticleManager.Instance.ExternalForceRange.x, ParticleManager.Instance.ExternalForceRange.y);
                    InternalMins[i, j] = Random.Range(ParticleManager.Instance.InternalMinDistanceRange.x, ParticleManager.Instance.InternalMinDistanceRange.y);
                    ExternalMins[i, j] = Random.Range(ParticleManager.Instance.ExternalMinDistanceRange.x, ParticleManager.Instance.ExternalMinDistanceRange.y);
                    InternalRadii[i, j] = Random.Range(ParticleManager.Instance.InternalRadiusRange.x, ParticleManager.Instance.InternalRadiusRange.y);
                    ExternalRadii[i, j] = Random.Range(ParticleManager.Instance.ExternalRadiusRange.x, ParticleManager.Instance.ExternalRadiusRange.y);
                }
            }

            // Set the maximum radii for quick reference
            MaxInternalRadii = ParticleManager.Instance.InternalRadiusRange.y;
            MaxExternalRadii = ParticleManager.Instance.ExternalRadiusRange.y;
        }

        private void GenerateNew(float x, float y)
        {
            // Clear the current swarm and generate new particles
            Swarm = new List<Particle>();

            for (int i = 0; i < numParticles; i++)
            {
                Vector3 spawnPos;

                do
                {
                    // Generate a small random offset
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-0.5f, 0.5f),
                        Random.Range(-0.5f, 0.5f),
                        0
                    );

                    spawnPos = new Vector3(x, y, 0) + randomOffset;
                }
                while (Vector2.Distance(spawnPos, _player.transform.position) < 2); // Continue generating new positions if they are too close to the player


                // Instantiate the particle at the cluster position plus the random offset
                GameObject particleObj = Instantiate(
                    _particlePrefab,
                    spawnPos,
                    Quaternion.identity
                );
                Particle newParticle = particleObj.GetComponent<Particle>();

                int particleType = Random.Range(0, _numTypes);

                // Initialize the particle
                newParticle.Initialize(particleType, this);
                newParticle.transform.parent = this.transform;
                Swarm.Add(newParticle);
            }

            // Adjust the center of the cluster
            AdjustCenter();
        }

        public void UpdateCluster()
        {
            // Remove null particles from the swarm
            Swarm.RemoveAll(p => p == null);

            // Adjust the center of the cluster
            AdjustCenter();

            // Apply forces and behaviors to each particle
            foreach (Particle particle in Swarm)
            {
                if (particle == null) continue;

                particle.ApplyInternalForces(this);
                particle.ApplyExternalForces(this);
                particle.ApplyPlayerAttraction();
                particle.ApplyCohesion();
            }
        }

        public void AdjustCenter()
        {
            // Adjust the center of the cluster based on the positions of the particles
            if (Swarm.Count == 0) return;

            Vector3 sum = Vector3.zero;

            foreach (Particle p in Swarm)
            {
                if (p == null) continue;
                sum += p.Position;
            }

            Center = sum / Swarm.Count;
        }

        public void UpdateForceParameters()
        {
            // Update the force matrices based on current ranges from ParticleManager
            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] = Random.Range(ParticleManager.Instance.InternalForceRange.x, ParticleManager.Instance.InternalForceRange.y);
                    ExternalForces[i, j] = Random.Range(ParticleManager.Instance.ExternalForceRange.x, ParticleManager.Instance.ExternalForceRange.y);
                    InternalMins[i, j] = Random.Range(ParticleManager.Instance.InternalMinDistanceRange.x, ParticleManager.Instance.InternalMinDistanceRange.y);
                    ExternalMins[i, j] = Random.Range(ParticleManager.Instance.ExternalMinDistanceRange.x, ParticleManager.Instance.ExternalMinDistanceRange.y);
                    InternalRadii[i, j] = Random.Range(ParticleManager.Instance.InternalRadiusRange.x, ParticleManager.Instance.InternalRadiusRange.y);
                    ExternalRadii[i, j] = Random.Range(ParticleManager.Instance.ExternalRadiusRange.x, ParticleManager.Instance.ExternalRadiusRange.y);
                }
            }

            // Update maximum radii in case ranges have changed
            MaxInternalRadii = ParticleManager.Instance.InternalRadiusRange.y;
            MaxExternalRadii = ParticleManager.Instance.ExternalRadiusRange.y;
        }

        public void ReportParticleProximity(float minimalDistance)
        {
            // Invert the minimal distance to make closer distances contribute more to the score
            float proximityContribution = 1f / (minimalDistance + 0.001f); // Adding a small value to prevent division by zero
            ProximityScore += proximityContribution;
            TotalParticlesDestroyed++;
        }
    }
}
