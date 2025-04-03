using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Cluster : MonoBehaviour
{
    public List<Particle> Swarm { get; set; } = new List<Particle>();
    public SerializedDictionary<ParticleType, int> ParticleTypeCounts = new SerializedDictionary<ParticleType, int>();
    [ShowNativeProperty] public int Id { get; set; }

    public Array2D<float> InternalForces;
    public Array2D<float> ExternalForces;
    public Array2D<float> InternalMins;
    public Array2D<float> ExternalMins;
    public Array2D<float> InternalRadii;
    public Array2D<float> ExternalRadii;

    public float MaxInternalRadii { get; set; }
    public float MaxExternalRadii { get; set; }
    public Vector3 Center;
    public float ActiveRadius; // Basically the distance of furthest particle from center

    private GameObject _particlePrefab;
    public int _numTypes;

    public void Initialize(float x, float y, SerializedDictionary<ParticleType, int> particleTypes)
    {
        _particlePrefab = ClusterSpawning.Instance.ParticlePrefab;
        _numTypes = ParticleManager.Instance.numberOfTypes;

        // Initialize the cluster with a specific position and particle types
        InitializeForceMatrices();
        GenerateParticles(x, y, particleTypes);
    }
    public void Initialize(float x, float y, int numberOfParticles)
    {
        _particlePrefab = ClusterSpawning.Instance.ParticlePrefab;
        _numTypes = ParticleManager.Instance.numberOfTypes;

        // Initialize force matrices and generate new particles
        InitializeForceMatrices();
        GenerateParticles(x, y, numberOfParticles);
    }

    public void InitializeForceMatrices()
    {
        // Initialize the force matrices based on the number of types
        InternalForces = new Array2D<float>(_numTypes, _numTypes);
        ExternalForces = new Array2D<float>(_numTypes, _numTypes + 1);
        InternalMins = new Array2D<float>(_numTypes, _numTypes);
        ExternalMins = new Array2D<float>(_numTypes, _numTypes + 1);
        InternalRadii = new Array2D<float>(_numTypes, _numTypes);
        ExternalRadii = new Array2D<float>(_numTypes, _numTypes + 1);

        // Temporarily cache ParticleManager ranges
        Vector2 internalForceRange = ParticleManager.Instance.InternalForceRange;
        Vector2 externalForceRange = ParticleManager.Instance.ExternalForceRange;
        Vector2 internalMinDistanceRange = ParticleManager.Instance.InternalMinDistanceRange;
        Vector2 externalMinDistanceRange = ParticleManager.Instance.ExternalMinDistanceRange;
        Vector2 internalRadiusRange = ParticleManager.Instance.InternalRadiusRange;
        Vector2 externalRadiusRange = ParticleManager.Instance.ExternalRadiusRange;

        // Initialize with default or random values
        for (int i = 0; i < _numTypes; i++)
        {
            for (int j = 0; j < _numTypes + 1; j++)
            {
                if (j >= _numTypes) // For the last loop (_numTypes + 1), only adjust external
                {
                    ExternalForces[i, j] = externalForceRange.y * ParticleManager.Instance.ForceMultiplier;
                    ExternalMins[i, j] = externalMinDistanceRange.y;
                    ExternalRadii[i, j] = externalRadiusRange.y;

                    break;
                }

                InternalForces[i, j] = UnityEngine.Random.Range(internalForceRange.x, internalForceRange.y) * ParticleManager.Instance.ForceMultiplier;
                InternalMins[i, j] = UnityEngine.Random.Range(internalMinDistanceRange.x, internalMinDistanceRange.y);
                InternalRadii[i, j] = UnityEngine.Random.Range(internalRadiusRange.x, internalRadiusRange.y);
                ExternalForces[i, j] = UnityEngine.Random.Range(externalForceRange.x, externalForceRange.y) * ParticleManager.Instance.ForceMultiplier;
                ExternalMins[i, j] = UnityEngine.Random.Range(externalMinDistanceRange.x, externalMinDistanceRange.y);
                ExternalRadii[i, j] = UnityEngine.Random.Range(externalRadiusRange.x, externalRadiusRange.y);
            }
        }

        // Since internal forces should all be positive, we want to make one type negative to add movement
        CreateInternalMovement();

        // Set the maximum radii for quick reference
        MaxInternalRadii = internalRadiusRange.y;
        MaxExternalRadii = externalRadiusRange.y;
    }

    private void CreateInternalMovement()
    {
        if(_numTypes < 2)
        {
            Debug.LogWarning("Less than two types detected. Cannot create internal movement.");
            return;
        }

        // This function manually assigns two types to have following behavior.
        // In other words, one type will attract towards another type, but the other type will repel from it.

        // Select two random types from the number of types, and make sure they are not the same type
        int type1, type2;
        do {
            type1 = UnityEngine.Random.Range(0, _numTypes); 
            type2 = UnityEngine.Random.Range(0, _numTypes);
        }
        while (type1 == type2);

        InternalForces[type1, type2] = -InternalForces[type2, type1] * 0.75f;
        InternalMins[type1, type2] = 1 / InternalRadii[type1, type2];
        InternalMins[type2, type1] = 1 / InternalRadii[type2, type1];
    }

    private void MutateForceMatrices(float mutationRate = 0.1f)
    {
        for (int i = 0; i < _numTypes; i++)
        {
            for (int j = 0; j < _numTypes + 1; j++)
            {
                if (j >= _numTypes) // For the last loop (_numTypes + 1), only adjust external
                {
                    ExternalForces[i, j] += Random.Range(-mutationRate, mutationRate);
                    ExternalMins[i, j] += Random.Range(-mutationRate, mutationRate);
                    ExternalRadii[i, j] += Random.Range(-mutationRate, mutationRate);

                    break;
                }

                InternalForces[i, j] += Random.Range(-mutationRate, mutationRate);
                InternalMins[i, j] += Random.Range(-mutationRate, mutationRate);
                InternalRadii[i, j] += Random.Range(-mutationRate, mutationRate);
                ExternalForces[i, j] += Random.Range(-mutationRate, mutationRate);
                ExternalMins[i, j] += Random.Range(-mutationRate, mutationRate);
                ExternalRadii[i, j] += Random.Range(-mutationRate, mutationRate);
            }
        }
    }

    public void Reproduce()
    {
        // Note that this automatically creates an exact copy of the cluster, including its values
        Cluster newCluster = Instantiate(gameObject, transform.position, Quaternion.identity, transform.parent).GetComponent<Cluster>();
        newCluster.Id = ParticleManager.Instance.RunningClusterCount++;
        newCluster.gameObject.name = $"Cluster {newCluster.Id} (Mutated from {Id})";

        newCluster.ResetSwarm();
        newCluster.MutateForceMatrices(0.3f);

        ParticleManager.Instance.Clusters.Add(newCluster);
    }

    private void Update()
    {
        if (Id == (ParticleManager.Instance.RunningClusterCount-1) && Input.GetKeyDown(KeyCode.L))
        {
            Reproduce();
        }
    }

    private SerializedDictionary<ParticleType, int> GenerateRandomParticles(int numberOfParticles)
    {
        // Generate a random number of particles of each type
        SerializedDictionary<ParticleType, int> particleCounts = new SerializedDictionary<ParticleType, int>();
        for (int i = 0; i < numberOfParticles; i++)
        {
            ParticleType randomType = (ParticleType)UnityEngine.Random.Range(0, _numTypes);
            if (!particleCounts.ContainsKey(randomType))
            {
                particleCounts[randomType] = 0; // Initialize count for this type if not present
            }
            particleCounts[randomType]++;
        }
        return particleCounts;
    }

    public void GenerateParticles(float x, float y, int numberOfParticles) => GenerateParticles(x, y, GenerateRandomParticles(numberOfParticles));
    public void GenerateParticles(float x, float y, SerializedDictionary<ParticleType, int> particleCounts)
    {
        var copy = particleCounts.Clone(); // SerializeDictionary pass by reference so we need to make a copy

        int numberOfParticles = 0;
        foreach (var kvp in copy)
        {
            ParticleTypeCounts.Add(kvp.Key, kvp.Value);
            numberOfParticles += kvp.Value;
        }

        // Clear the current swarm and generate new particles
        Swarm = new List<Particle>();

        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector3 spawnPos;

            // Generate a small random offset
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0
            );

            spawnPos = new Vector3(x, y, 0) + randomOffset;

            // Instantiate the particle at the cluster position plus the random offset
            GameObject particleObj = Instantiate(
                _particlePrefab,
                spawnPos,
                Quaternion.identity
            );
            Particle newParticle = particleObj.GetComponent<Particle>();

            // Assign type properly
            ParticleType type = ParticleType.Neutral; // Default type
            foreach (var kvp in copy)
            {
                if (kvp.Value > 0)
                {
                    type = kvp.Key; // Get the first available type
                    copy[kvp.Key]--; // Decrease the count for this type
                    break;
                }
            }

            // Initialize the particle
            newParticle.Initialize(this, i, type);
            particleObj.name = "Particle " + i + " (" + type.ToString() + ")";

            newParticle.transform.parent = this.transform;
            Swarm.Add(newParticle);
        }

        // Adjust the center of the cluster
        AdjustCenter();
    }

    public void UpdateCluster()
    {
        // Adjust the center of the cluster
        AdjustCenter();

        // Apply forces and behaviors to each particle
        foreach (Particle particle in Swarm)
        {
            if (particle == null) continue;

            Debug.Log($"Cluster {Id} running update.");

            particle.ApplyInternalForces(this);
            particle.ApplyExternalForces(this);
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
            sum += p.position;
        }

        Center = sum / Swarm.Count;

        // Update the active radius based on the furthest particle from the center
        ActiveRadius = 0f;
        foreach (Particle p in Swarm)
        {
            if (p == null) continue;

            float distance = Vector3.Distance(Center, p.position);
            if (distance > ActiveRadius)
            {
                ActiveRadius = distance;
            }
        }
    }

    private void ResetSwarm()
    {
        Swarm = new List<Particle>();

        foreach(var particle in GetComponentsInChildren<Particle>())
        {
            particle.ParentCluster = this;
            Swarm.Add(particle);
        }
    }

    // Draw debug lines and circles
    private void OnDrawGizmos()
    {
        if (Swarm == null) return;

        // Draw lines between particles
        if (ParticleManager.Instance.DrawParticleLines)
        {
            for (int i = 0; i < Swarm.Count; i++)
            {
                for (int j = i + 1; j < Swarm.Count; j++)
                {
                    if (Swarm[i] == null || Swarm[j] == null) continue;
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(Swarm[i].position, Swarm[j].position);
                }
            }
        }

        // Draw circles visualizing radius
        if (ParticleManager.Instance.DrawClusterCircles)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Center, ActiveRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Center, 0.1f);
        }
    }
}