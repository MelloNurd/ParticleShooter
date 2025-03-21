using UnityEngine;
using NaughtyAttributes;
using System;

public class Particle : MonoBehaviour
{
    public Vector3 position = Vector3.zero;

    public Vector3 velocity;
    public ParticleStats stats;

    public ParticleType type;

    public Cluster ParentCluster;

    private Transform _transform;
    private SpriteRenderer _spriteRenderer;

    private Transform _playerTransform;


    public void SetColor() => SetColor(stats.GetColorByType());
    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    private void Awake()
    {
        // Cache the transform and sprite renderer components
        _transform = GetComponent<Transform>();
        position = _transform.position;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        _playerTransform = ParticleManager.Instance.player.transform;
    }

    public void Initialize(Cluster parentCluster, int id) => Initialize(parentCluster, id, (ParticleType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ParticleType)).Length));
    public void Initialize(Cluster parentCluster, int id, ParticleType type)
    {
        ParentCluster = parentCluster;

        // Set initial velocity 
        velocity = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            0
        );

        stats = new ParticleStats(this, id, type);

        SetColor();
    }

    private void Update()
    {
        // Original behavior for active particles
        ApplyInternalForces(ParentCluster);
        ApplyExternalForces(ParentCluster);
        ApplyCohesion();

        position += velocity * Time.deltaTime;
        _transform.position = position;
    }


    private void OnDestroy()
    {
        // Remove from Swarm
        if (ParentCluster != null && ParentCluster.Swarm != null)
        {
            ParentCluster.Swarm.Remove(this);
        }
    }

    public void ApplyInternalForces(Cluster cluster)
    {
        if (this == null || _transform == null || cluster == null) return;

        Vector3 totalForce = Vector3.zero;

        foreach (Particle particle in cluster.Swarm)
        {
            if (particle == this || particle == null) continue;

            Vector3 direction = particle.position - position;

            float distance = direction.magnitude;
            direction.Normalize();

            // Repulsive forces
            if (distance < cluster.InternalMins[stats.TypeInt, particle.stats.TypeInt])
            {
                Vector3 force = direction * Mathf.Abs(cluster.InternalForces[stats.TypeInt, particle.stats.TypeInt]) * ParticleManager.Instance.RepulsionEffector;
                force *= Map(distance, 0, Mathf.Abs(cluster.InternalMins[stats.TypeInt, particle.stats.TypeInt]), 1, 0) * ParticleManager.Instance.Dampening;
                totalForce += force;
            }

            // Attractive forces
            if (distance < cluster.InternalRadii[stats.TypeInt, particle.stats.TypeInt])
            {
                Vector3 force = direction * cluster.InternalForces[stats.TypeInt, particle.stats.TypeInt];
                force *= Map(distance, 0, cluster.InternalRadii[stats.TypeInt, particle.stats.TypeInt], 1, 0) * ParticleManager.Instance.Dampening;
                totalForce += force;
            }
        }

        // Apply forces smoothly
        velocity += totalForce * Time.deltaTime;
        position += velocity * Time.deltaTime;
        velocity *= ParticleManager.Instance.Friction;

        if (_transform != null)
            _transform.position = position;
    }

    public void ApplyExternalForces(Cluster cluster)
    {
        if (this == null || _transform == null || cluster == null) return;

        Vector3 totalForce = Vector3.zero;

        foreach (Cluster otherCluster in ParticleManager.Instance.Clusters)
        {
            if (otherCluster == cluster || otherCluster == null) continue;
            if (Vector2.Distance(cluster.Center, otherCluster.Center) > cluster.MaxExternalRadii) continue;

            foreach (Particle particle in otherCluster.Swarm)
            {
                if (particle == null) continue;

                Vector3 direction = particle.position - position;

                float distance = direction.magnitude;
                direction.Normalize();

                // Repulsive forces
                if (distance < cluster.ExternalMins[stats.TypeInt, particle.stats.TypeInt])
                {
                    Vector3 force = direction * Mathf.Abs(cluster.ExternalForces[stats.TypeInt, particle.stats.TypeInt]) * ParticleManager.Instance.RepulsionEffector;
                    force *= Map(distance, 0, Mathf.Abs(cluster.ExternalMins[stats.TypeInt, particle.stats.TypeInt]), 1, 0) * ParticleManager.Instance.Dampening;
                    totalForce += force;
                }

                // Attractive forces 
                if (distance < cluster.ExternalRadii[stats.TypeInt, particle.stats.TypeInt])
                {
                    Vector3 force = direction * cluster.ExternalForces[stats.TypeInt, particle.stats.TypeInt];
                    force *= Map(distance, 0, cluster.ExternalRadii[stats.TypeInt, particle.stats.TypeInt], 1, 0) * ParticleManager.Instance.Dampening;
                    totalForce += force;
                }
            }
        }

        // Apply forces smoothly
        velocity += totalForce * Time.deltaTime;
        position += velocity * Time.deltaTime;
        velocity *= ParticleManager.Instance.Friction;

        if (_transform != null)
            _transform.position = position;
    }

    private float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        value = Mathf.Clamp(value, inMin, inMax);
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    public void ApplyCohesion()
    {
        if (ParentCluster == null) return;

        Vector3 direction = ParentCluster.Center - position;

        float distance = direction.magnitude;
        direction.Normalize();

        // Use the CohesionStrength from ParticleManager
        float cohesionStrength = ParticleManager.Instance.CohesionStrength;

        // Apply cohesion force
        Vector3 cohesionForce = direction * cohesionStrength;

        velocity += cohesionForce * Time.deltaTime;
    }
}