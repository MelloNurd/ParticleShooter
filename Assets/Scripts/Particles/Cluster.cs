using NUnit.Framework;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
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

        public float MaxInternalRadii { get; set; } = 0;
        public float MaxExternalRadii { get; set; } = 0;

        public Vector3 Center;

        public int numParticles = 40;
        private GameObject _particlePrefab;
        private int _numTypes;

        private void Awake()
        {
            _numTypes = ParticleManager.Instance.NumberOfTypes;
            _particlePrefab = ParticleManager.Instance.ParticlePrefab;
        }

        public void Initialize(float x, float y)
        {
            var manager = ParticleManager.Instance;
            if (manager == null)
            {
                Debug.LogError("ParticleManager Instance is null during Cluster initialization.");
                return;
            }

            _numTypes = manager.NumberOfTypes; // Ensure _numTypes is set

            InternalForces = new float[_numTypes, _numTypes];
            ExternalForces = new float[_numTypes, _numTypes];
            InternalMins = new float[_numTypes, _numTypes];
            ExternalMins = new float[_numTypes, _numTypes];
            InternalRadii = new float[_numTypes, _numTypes];
            ExternalRadii = new float[_numTypes, _numTypes];

            GenerateNew(x, y);
        }

        private void GenerateNew(float x, float y)
        {
            var manager = ParticleManager.Instance;

            // Initialize interaction matrices
            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] = UnityEngine.Random.Range(manager.InternalForceRange.x, manager.InternalForceRange.y);
                    InternalMins[i, j] = UnityEngine.Random.Range(manager.InternalMinDistanceRange.x, manager.InternalMinDistanceRange.y);
                    InternalRadii[i, j] = UnityEngine.Random.Range(manager.InternalRadiusRange.x, manager.InternalRadiusRange.y);

                    ExternalForces[i, j] = UnityEngine.Random.Range(manager.ExternalForceRange.x, manager.ExternalForceRange.y);
                    ExternalMins[i, j] = UnityEngine.Random.Range(manager.ExternalMinDistanceRange.x, manager.ExternalMinDistanceRange.y);
                    ExternalRadii[i, j] = UnityEngine.Random.Range(manager.ExternalRadiusRange.x, manager.ExternalRadiusRange.y);

                    if (InternalRadii[i, j] > MaxInternalRadii)
                        MaxInternalRadii = InternalRadii[i, j];
                    if (ExternalRadii[i, j] > MaxExternalRadii)
                        MaxExternalRadii = ExternalRadii[i, j];
                }
            }

            // Instantiate particles
            for (int i = 0; i < numParticles; i++)
            {
                Vector3 position = new Vector3(
                    x + UnityEngine.Random.Range(-0.5f, 0.5f),
                    y + UnityEngine.Random.Range(-0.5f, 0.5f),
                    0);

                if (manager.ParticlePrefab == null)
                {
                    Debug.LogError("ParticlePrefab is not assigned in ParticleManager.");
                    return;
                }

                GameObject particleObj = Instantiate(manager.ParticlePrefab, position, Quaternion.identity, transform);
                Particle newParticle = particleObj.GetComponent<Particle>();

                if (newParticle == null)
                {
                    Debug.LogError("Particle component missing on ParticlePrefab.");
                    continue;
                }

                newParticle.Position = position;
                newParticle.Type = UnityEngine.Random.Range(0, _numTypes);
                newParticle.ParentCluster = this;

                // Set color based on type
                SpriteRenderer sr = newParticle.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.HSVToRGB((float)newParticle.Type / _numTypes, 1, 1);
                }
                else
                {
                    Debug.LogWarning("SpriteRenderer missing on Particle.");
                }

                Swarm.Add(newParticle);
            }
        }



        public void GenerateFromSuccessData(Dictionary<int, int> successData)
        {
            if (successData == null || successData.Count == 0)
            {
                Debug.LogWarning("GenerateFromSuccessData: successData is null or empty. Generating with random types.");
            }

            // Clear existing particles
            foreach (var particle in Swarm)
            {
                Destroy(particle.gameObject);
            }
            Swarm.Clear();

            // Calculate total hits
            int totalHits = 0;
            foreach (var count in successData.Values)
                totalHits += count;

            // Calculate type ratios based on success
            Dictionary<int, float> typeRatios = new Dictionary<int, float>();
            foreach (var kvp in successData)
                typeRatios[kvp.Key] = (float)kvp.Value / totalHits;

            // Instantiate particles based on success data
            for (int i = 0; i < numParticles; i++)
            {
                Vector3 position = Center + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0);

                if (ParticleManager.Instance.ParticlePrefab == null)
                {
                    Debug.LogError("ParticlePrefab is not assigned in ParticleManager.");
                    return;
                }

                GameObject particleObj = Instantiate(ParticleManager.Instance.ParticlePrefab, position, Quaternion.identity, transform);
                Particle newParticle = particleObj.GetComponent<Particle>();

                if (newParticle == null)
                {
                    Debug.LogError("Particle component missing on ParticlePrefab.");
                    continue;
                }

                // Select type based on weighted random
                float rand = UnityEngine.Random.value;
                float cumulative = 0f;
                bool typeAssigned = false;
                foreach (var kvp in typeRatios)
                {
                    cumulative += kvp.Value;
                    if (rand <= cumulative)
                    {
                        newParticle.Type = kvp.Key;
                        typeAssigned = true;
                        break;
                    }
                }

                if (!typeAssigned)
                {
                    newParticle.Type = UnityEngine.Random.Range(0, _numTypes);
                    Debug.LogWarning("GenerateFromSuccessData: Unable to assign type based on success data. Assigning random type.");
                }

                newParticle.Position = position;
                newParticle.ParentCluster = this;

                // Set color based on type
                SpriteRenderer sr = newParticle.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.HSVToRGB((float)newParticle.Type / _numTypes, 1, 1);
                }
                else
                {
                    Debug.LogWarning("SpriteRenderer missing on Particle.");
                }

                Swarm.Add(newParticle);
            }

            // Reinitialize Max radii
            MaxInternalRadii = 0;
            MaxExternalRadii = 0;
            foreach (var particle in Swarm)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    if (InternalRadii[particle.Type, j] > MaxInternalRadii)
                        MaxInternalRadii = InternalRadii[particle.Type, j];
                    if (ExternalRadii[particle.Type, j] > MaxExternalRadii)
                        MaxExternalRadii = ExternalRadii[particle.Type, j];
                }
            }
        }





        public void UpdateForceParameters()
        {
            var manager = ParticleManager.Instance;

            MaxInternalRadii = 0;
            MaxExternalRadii = 0;

            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] = Mathf.Clamp(InternalForces[i, j], manager.InternalForceRange.x, manager.InternalForceRange.y);
                    InternalMins[i, j] = Mathf.Clamp(InternalMins[i, j], manager.InternalMinDistanceRange.x, manager.InternalMinDistanceRange.y);
                    InternalRadii[i, j] = Mathf.Clamp(InternalRadii[i, j], manager.InternalRadiusRange.x, manager.InternalRadiusRange.y);

                    ExternalForces[i, j] = Mathf.Clamp(ExternalForces[i, j], manager.ExternalForceRange.x, manager.ExternalForceRange.y);
                    ExternalMins[i, j] = Mathf.Clamp(ExternalMins[i, j], manager.ExternalMinDistanceRange.x, manager.ExternalMinDistanceRange.y);
                    ExternalRadii[i, j] = Mathf.Clamp(ExternalRadii[i, j], manager.ExternalRadiusRange.x, manager.ExternalRadiusRange.y);

                    if (InternalRadii[i, j] > MaxInternalRadii)
                        MaxInternalRadii = InternalRadii[i, j];
                    if (ExternalRadii[i, j] > MaxExternalRadii)
                        MaxExternalRadii = ExternalRadii[i, j];
                }
            }
        }

        public void UpdateCluster()
        {
            // Remove null particles
            Swarm.RemoveAll(p => p == null);

            // Process remaining particles
            for (int i = Swarm.Count - 1; i >= 0; i--)
            {
                Particle p = Swarm[i];
                if (p == null)
                {
                    Swarm.RemoveAt(i);
                    continue;
                }

                p.ApplyInternalForces(this);
                p.ApplyExternalForces(this);
                p.ApplyCohesion();
                p.ApplyPlayerAttraction();

            }

            AdjustCenter();
        }

        public void AdjustCenter()
        {
            if (Swarm.Count == 0) return;

            Vector3 sum = Vector3.zero;
            foreach (var p in Swarm)
            {
                sum += p.Position;
            }

            Center = sum / Swarm.Count;
        }

        // Mutation based on particle success
        public void MutateCluster(Dictionary<int, int> successData)
        {
            // Adjust interaction matrices based on success
            foreach (var kvp in successData)
            {
                int type = kvp.Key;
                float successRate = (float)kvp.Value / numParticles;

                // Mutate internal forces
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[type, j] += UnityEngine.Random.Range(-0.1f, 0.1f) * successRate;
                    InternalMins[type, j] += UnityEngine.Random.Range(-0.05f, 0.05f) * successRate;
                    InternalRadii[type, j] += UnityEngine.Random.Range(-0.05f, 0.05f) * successRate;
                }

                // Mutate external forces
                for (int j = 0; j < _numTypes; j++)
                {
                    ExternalForces[type, j] += UnityEngine.Random.Range(-0.1f, 0.1f) * successRate;
                    ExternalMins[type, j] += UnityEngine.Random.Range(-0.05f, 0.05f) * successRate;
                    ExternalRadii[type, j] += UnityEngine.Random.Range(-0.05f, 0.05f) * successRate;
                }
            }

            // Mutate particles
            foreach (Particle p in Swarm)
            {
                if (UnityEngine.Random.value < 0.1f) // 10% chance to change type
                {
                    p.Type = UnityEngine.Random.Range(0, _numTypes);
                    p.GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)p.Type / _numTypes, 1, 1);
                }
            }
        }
    }
}
