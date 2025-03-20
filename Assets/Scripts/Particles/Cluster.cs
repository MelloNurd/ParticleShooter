using System.Collections.Generic;
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

    public int HitsToPlayer { get; set; } = 0;

    public int numParticles = 40;

    private GameObject _particlePrefab;
    public int _numTypes;

    public float ProximityScore { get; private set; }
    public int TotalParticlesDestroyed { get; private set; }

    private Player _player;

    // Add these fields to manage attack timing
    private float _attackCooldown = 3f; // Time between attacks
    private float _attackTimer = 3f;    // Timer to track cooldown
    private float _attackRange = 10f;   // Range within which the cluster can attack

    private void Awake()
    {
        // Initialize particle prefab and number of types from ParticleManager
        _particlePrefab = ParticleManager.Instance.ParticlePrefab;
        _numTypes = ParticleManager.Instance.numberOfTypes;
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
        InternalForces = new Array2D<float>(_numTypes, _numTypes);
        ExternalForces = new Array2D<float>(_numTypes, _numTypes);
        InternalMins = new Array2D<float>(_numTypes, _numTypes);
        ExternalMins = new Array2D<float>(_numTypes, _numTypes);
        InternalRadii = new Array2D<float>(_numTypes, _numTypes);
        ExternalRadii = new Array2D<float>(_numTypes, _numTypes);

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

            // Generate a small random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
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

            // Initialize the particle
            ParticleType type = newParticle.Initialize(this, i);
            particleObj.name = "Particle " + i + " (" + type.ToString() + ")";

            newParticle.transform.parent = this.transform;
            Swarm.Add(newParticle);
            if(!ParticleTypeCounts.ContainsKey(type))
            {
                ParticleTypeCounts[type] = 0; // Initialize count for this type if not present
            }
            ParticleTypeCounts[type]++;
        }

        foreach (var kvp in ParticleTypeCounts)
        {
            Debug.Log($"Particle Type: {kvp.Key}, Count: {kvp.Value}");
        }

        // Adjust the center of the cluster
        AdjustCenter();

        _attackTimer = Random.Range(3f, 7f); // Add some randomness to the cooldown
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
            // particle.ApplyExternalForces(this);
            particle.ApplyPlayerAttraction();
            particle.ApplyCohesion();
        }

        // Update the attack timer
        _attackTimer -= Time.deltaTime;

        // Check if the cluster is near the player and ready to attack
        if (_attackTimer <= 0f && Vector3.Distance(Center, _player.transform.position) <= _attackRange)
        {
            // Check if there are any projectile particles available
            if (Swarm.Exists(p => p.type == 0))
            {
                // Launch a particle at the player
                LaunchParticleAtPlayer(5f, 15f);

                // Reset the attack timer
                _attackTimer = _attackCooldown;
                _attackTimer += Random.Range(3f, 7f); // Add some randomness to the cooldown
            }
            else
            {
                // No projectile particles left, optionally adjust behavior
                // For example, set a shorter cooldown before checking again
                _attackTimer = 1f;
            }
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

    public void LaunchParticleAtPlayer(float launchSpeed, float maxTravelDistance)
    {
        if (Swarm.Count == 0) return;

        int projectileType = 0; // The particle type designated as the projectile

        // Find a particle of the projectile type in the swarm
        Particle particleToLaunch = Swarm.Find(p => p.stats.TypeInt == projectileType);

        if (particleToLaunch == null)
        {
            // No particles of the projectile type are available
            return;
        }

        // Remove the particle from the swarm
        Swarm.Remove(particleToLaunch);

        // Detach the particle from the cluster
        particleToLaunch.transform.parent = null;

        // Calculate the direction towards the player
        Vector3 direction = (_player.transform.position - particleToLaunch.position).normalized;

        // Launch the particle
        particleToLaunch.Launch(direction, launchSpeed, maxTravelDistance);

        // Set the particle's color to white
        particleToLaunch.SetColor(Color.white);

        // Mark the particle as a projectile
        particleToLaunch.IsProjectile = true;
    }
}