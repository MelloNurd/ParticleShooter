using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Cluster : MonoBehaviour
{
    public List<Particle> Swarm { get; set; } = new List<Particle>();
    public Dictionary<ParticleType, int> ParticleTypeCounts = new Dictionary<ParticleType, int>();
    public int Id { get; set; }

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

    private Player _player;

    private void Awake()
    {
        // Initialize particle prefab and number of types from ParticleManager
        _particlePrefab = ParticleManager.Instance.ParticlePrefab;
        _numTypes = ParticleManager.Instance.numberOfTypes;
        _player = FindFirstObjectByType<Player>();
    }

    public void Initialize(float x, float y, Dictionary<ParticleType, int> particleTypes)
    {
        // Initialize the cluster with a specific position and particle types
        InitializeForceMatrices();
        GenerateParticles(x, y, particleTypes);
    }
    public void Initialize(float x, float y, int numberOfParticles)
    {
        // Initialize force matrices and generate new particles
        InitializeForceMatrices();
        GenerateParticles(x, y, numberOfParticles);
    }

    private void InitializeForceMatrices()
    {
        // Initialize the force matrices based on the number of types
        InternalForces = new Array2D<float>(_numTypes, _numTypes);
        ExternalForces = new Array2D<float>(_numTypes, _numTypes + 1);
        InternalMins = new Array2D<float>(_numTypes, _numTypes);
        ExternalMins = new Array2D<float>(_numTypes, _numTypes + 1);
        InternalRadii = new Array2D<float>(_numTypes, _numTypes);
        ExternalRadii = new Array2D<float>(_numTypes, _numTypes + 1);

        // Initialize with default or random values
        for (int i = 0; i < _numTypes; i++)
        {
            for (int j = 0; j < _numTypes + 1; j++)
            {
                if(j < _numTypes) // We only apply the last column to External Forces
                {
                    InternalForces[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.InternalForceRange.x, ParticleManager.Instance.InternalForceRange.y);
                    InternalMins[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.InternalMinDistanceRange.x, ParticleManager.Instance.InternalMinDistanceRange.y);
                    InternalRadii[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.InternalRadiusRange.x, ParticleManager.Instance.InternalRadiusRange.y);
                }

                ExternalForces[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.ExternalForceRange.x, ParticleManager.Instance.ExternalForceRange.y);
                ExternalMins[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.ExternalMinDistanceRange.x, ParticleManager.Instance.ExternalMinDistanceRange.y);
                ExternalRadii[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.ExternalRadiusRange.x, ParticleManager.Instance.ExternalRadiusRange.y);
            }
        }

        // Set the maximum radii for quick reference
        MaxInternalRadii = ParticleManager.Instance.InternalRadiusRange.y;
        MaxExternalRadii = ParticleManager.Instance.ExternalRadiusRange.y;
    }


    private Dictionary<ParticleType, int> GenerateRandomParticles(int numberOfParticles)
    {
        // Generate a random number of particles of each type
        Dictionary<ParticleType, int> particleCounts = new Dictionary<ParticleType, int>();
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
    public void GenerateParticles(float x, float y, Dictionary<ParticleType, int> particleCounts)
    {
        ParticleTypeCounts = particleCounts;

        int numberOfParticles = 0;
        foreach (var kvp in particleCounts)
        {
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
            foreach (var kvp in particleCounts)
            {
                if (kvp.Value > 0)
                {
                    type = kvp.Key; // Get the first available type
                    particleCounts[kvp.Key]--; // Decrease the count for this type
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

    public void UpdateForceParameters()
    {
        // Update the force matrices based on current ranges from ParticleManager
        for (int i = 0; i < _numTypes; i++)
        {
            for (int j = 0; j < _numTypes; j++)
            {
                InternalForces[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.InternalForceRange.x, ParticleManager.Instance.InternalForceRange.y);
                ExternalForces[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.ExternalForceRange.x, ParticleManager.Instance.ExternalForceRange.y);
                InternalMins[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.InternalMinDistanceRange.x, ParticleManager.Instance.InternalMinDistanceRange.y);
                ExternalMins[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.ExternalMinDistanceRange.x, ParticleManager.Instance.ExternalMinDistanceRange.y);
                InternalRadii[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.InternalRadiusRange.x, ParticleManager.Instance.InternalRadiusRange.y);
                ExternalRadii[i, j] = UnityEngine.Random.Range(ParticleManager.Instance.ExternalRadiusRange.x, ParticleManager.Instance.ExternalRadiusRange.y);
            }
        }

        // Update maximum radii in case ranges have changed
        MaxInternalRadii = ParticleManager.Instance.InternalRadiusRange.y;
        MaxExternalRadii = ParticleManager.Instance.ExternalRadiusRange.y;
    }
}