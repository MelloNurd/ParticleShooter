using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Cluster : MonoBehaviour
{
    public Entity ParticleEntityPrefab;

    public List<Particle> Swarm { get; set; } = new List<Particle>();
    public SerializedDictionary<ParticleType, int> ParticleTypeCounts = new SerializedDictionary<ParticleType, int>();
    [ShowNativeProperty] public int Id { get; set; }

    public Array2D<float> InternalForces;
    public Array2D<float> ExternalForces;
    public Array2D<float> InternalMins;
    public Array2D<float> ExternalMins;
    public Array2D<float> InternalRadii;
    public Array2D<float> ExternalRadii;

    public List<GameObject> NearbyCrystals = new();

    public float lifetime = 0;
    public float energy = 100;
    public int eatEnergy;
    private float energyTimer;
    private int energyDecayRate = 2;

    private GameObject player;

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
        player = Player.Instance.gameObject;
        ParticleEntityPrefab = ParticleManager.Instance.ParticleEntityPrefab;

        // Initialize the cluster with a specific position and particle types
        InitializeForceMatrices();
        GenerateParticles(x, y, particleTypes);

    }
    public void Initialize(float x, float y, int numberOfParticles)
    {
        _particlePrefab = ClusterSpawning.Instance.ParticlePrefab;
        _numTypes = ParticleManager.Instance.numberOfTypes;
        player = Player.Instance.gameObject;
        ParticleEntityPrefab = ParticleManager.Instance.ParticleEntityPrefab;

        // Initialize force matrices and generate new particles
        InitializeForceMatrices();
        GenerateParticles(x, y, numberOfParticles);

    }

    public void InitializeForceMatrices()
    {
        // Initialize the force matrices based on the number of types
        InternalForces = new Array2D<float>(_numTypes, _numTypes);
        ExternalForces = new Array2D<float>(_numTypes, _numTypes + 2);
        InternalMins = new Array2D<float>(_numTypes, _numTypes);
        ExternalMins = new Array2D<float>(_numTypes, _numTypes + 2);
        InternalRadii = new Array2D<float>(_numTypes, _numTypes);
        ExternalRadii = new Array2D<float>(_numTypes, _numTypes + 2);

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
            for (int j = 0; j < _numTypes + 2; j++)
            {
                if (j >= _numTypes) // For the last loop (_numTypes + 1), only adjust external
                { // +1 is player, +2 is crystals
                    ExternalForces[i, j] = externalForceRange.y * ParticleManager.Instance.ForceMultiplier;
                    ExternalMins[i, j] = externalMinDistanceRange.y;
                    ExternalRadii[i, j] = externalRadiusRange.y;

                    continue;
                }

                InternalForces[i, j] = UnityEngine.Random.Range(internalForceRange.x, internalForceRange.y) * ParticleManager.Instance.ForceMultiplier;
                InternalMins[i, j] = UnityEngine.Random.Range(internalMinDistanceRange.x, internalMinDistanceRange.y);
                InternalRadii[i, j] = UnityEngine.Random.Range(internalRadiusRange.x, internalRadiusRange.y);
                ExternalForces[i, j] = UnityEngine.Random.Range(externalForceRange.x, externalForceRange.y) * ParticleManager.Instance.ForceMultiplier;
                ExternalMins[i, j] = UnityEngine.Random.Range(externalMinDistanceRange.x, externalMinDistanceRange.y);
                ExternalRadii[i, j] = UnityEngine.Random.Range(externalRadiusRange.x, externalRadiusRange.y);
            }
        }

        // Set the maximum radii for quick reference
        MaxInternalRadii = internalRadiusRange.y;
        MaxExternalRadii = externalRadiusRange.y;

        eatEnergy = UnityEngine.Random.Range(50, 100);
    }

    private void MutateForceMatrices(float mutationRate = 0.1f)
    {
        for (int i = 0; i < _numTypes; i++)
        {
            for (int j = 0; j < _numTypes + 2; j++)
            {
                if (j >= _numTypes) // For the last loop (_numTypes + 1), only adjust external
                {
                    ExternalForces[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                    ExternalMins[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                    ExternalRadii[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);

                    continue;
                }

                InternalForces[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                InternalMins[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                InternalRadii[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                ExternalForces[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                ExternalMins[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
                ExternalRadii[i, j] += UnityEngine.Random.Range(-mutationRate, mutationRate);
            }
        }

        eatEnergy += UnityEngine.Random.Range(-2, 3);
    }


    public Cluster Reproduce() => Reproduce(Center);
    public Cluster Reproduce(Vector3 position)
    {
        // Note that this automatically creates an exact copy of the cluster, including its values
        Cluster newCluster = Instantiate(gameObject, position, Quaternion.identity, transform.parent).GetComponent<Cluster>();
        newCluster.Id = ParticleManager.Instance.RunningClusterCount++;
        newCluster.gameObject.name = $"Cluster {newCluster.Id} (Mutated from {Id})";
        newCluster.Center = position;

        for(int i = 0; i < newCluster.transform.childCount; i++)
        {
            var temp = newCluster.transform.GetChild(i).GetComponent<Particle>();
            Debug.Log($"Mutated cluster particle {i}: {temp.type}");
        }

        List<Vector3> particlePositions = new();
        particlePositions.Add(Center);
        foreach (var particle in Swarm)
        {
            particlePositions.Add(particle.position);
        }

        newCluster.ResetSwarm(particlePositions);
        newCluster.MutateForceMatrices(0.3f);

        ParticleManager.Instance.Clusters.Add(newCluster);

        return newCluster;
    }

    private void Update()
    {
        if (Id == (ParticleManager.Instance.RunningClusterCount-1) && Input.GetKeyDown(KeyCode.L))
        {
            Reproduce();
        }

        // Lifetime increment
        lifetime += Time.deltaTime;

        // Energy decay
        if (energy > 0)
        {
            energy -= energyDecayRate * Time.deltaTime;
        }
        else if(!Utilities.IsOnScreen(Center, 5f)) // Try to avoid killing one on screen for immersion
        {
            KillCluster();
        }

            // Check for nearby crystals
            NearbyCrystals.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Center, 20f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Crystal"))
            {
                NearbyCrystals.Add(collider.gameObject);
            }
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

        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Clear Swarm
        Swarm = new List<Particle>(); // Not really used anymore in ECS-only world

        for (int i = 0; i < numberOfParticles; i++)
        {
            Vector3 spawnPos = new Vector3(
                x + UnityEngine.Random.Range(-0.5f, 0.5f),
                y + UnityEngine.Random.Range(-0.5f, 0.5f),
                0f
            );

            // Create entity
            Entity particle = em.Instantiate(ParticleEntityPrefab);

            // Pick type
            ParticleType type = ParticleType.Neutral;
            foreach (var kvp in copy)
            {
                if (kvp.Value > 0)
                {
                    type = kvp.Key;
                    copy[kvp.Key]--;
                    break;
                }
            }
        }

        // Center adjustment could still work if you query all entities owned by this cluster
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

    private void KillCluster()
    {
        // Destroy all particles in the swarm
        foreach (Particle particle in Swarm)
        {
            if (particle != null)
            {
                Destroy(particle.gameObject);
            }
        }

        ParticleManager.Instance.Clusters.Remove(this);

        // Destroy the cluster itself
        Destroy(gameObject);
    }

    private void ResetSwarm(List<Vector3> positions)
    {
        Swarm = new List<Particle>();

        Center = positions[0];

        int tempId = 0;
        foreach(var particle in GetComponentsInChildren<Particle>())
        {
            particle.stats = new ParticleStats(particle, tempId++, particle.type);
            particle.position = positions[tempId];
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