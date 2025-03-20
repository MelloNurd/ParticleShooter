using Mono.Cecil;
using System;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public ParticleTypes Type;
    public int Id;
    public float Health = 10f; // Health of the particle

    private Rigidbody2D _rb;

    private Vector2 force;

    public Cluster Cluster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        Initialize();
    }

    public void Initialize()
    {
        if(Cluster == null)
        {
            // Cluster = NewCluster.Instance; // We want to avoid this if possible. If this is happening, we're doing something wrong.
            Debug.LogError("Cluster is not set for the particle. Please assign it in the inspector or through code.");
            return;
        }
        Cluster.Particles.Add(this); // Add this particle to the cluster's list
        Cluster.ParticleTypeCounts[Type]++; // Increment the count for this type
    }

    // Update is called once per frame
    void Update()
    {
        // Compute forces from nearby particles as before
        force = Vector2.zero;

        // 1. Compute the natural attraction forces from neighboring particles.
        Vector2 interactionForce = Vector2.zero;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out Particle otherParticle))
            {
                float distance = Vector2.Distance(transform.position, otherParticle.transform.position);
                Vector2 direction = (otherParticle.transform.position - transform.position).normalized;
                interactionForce += direction * CalculateForce(distance, Cluster.AttractionMatrix[(int)Type, (int)otherParticle.Type]);
            }
        }

        // 2. Compute the arrival steering force based on distance to the cluster's target.
        Vector2 targetPos = (Vector2)Cluster.transform.position;
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float distanceToTarget = toTarget.magnitude;

        // Compute the base desired speed
        float baseDesiredSpeed = distanceToTarget * Cluster.ParticleSpeed;

        // Get the global count of speed particles (type 2)
        int speedParticleCount = Cluster.ParticleTypeCounts[ParticleTypes.Speed];
        float speedBonusPerParticle = 0.2f; // 10% bonus per speed particle, adjust as needed

        // Compute a multiplier so that more type 2 particles boost the speed.
        float speedMultiplier = 1.0f + (speedParticleCount * speedBonusPerParticle);

        // Calculate the final desired speed with bonus, clamped by maxSpeed.
        float desiredSpeed = Mathf.Min(baseDesiredSpeed * speedMultiplier, Cluster.ParticleMaxSpeed * speedMultiplier);

        if(Id == 2)
        {
            Debug.Log($"Id: {Id}, SpeedParticleCount: {speedParticleCount}, SpeedMultiplier: {speedMultiplier}, DesiredSpeed: {desiredSpeed}");
        }

        // Compute the desired velocity vector.
        Vector2 desiredVelocity = toTarget.normalized * desiredSpeed;

        // Compute the steering force (PD control could be used here if desired)
        Vector2 steeringForce = (desiredVelocity - _rb.linearVelocity) * Cluster.ParticleSteeringStrength;

        // 3. Combine the natural interaction force with a weighted arrival force.
        float arrivalWeight = 0.1f; // Lower weight to ensure natural cell behavior remains dominant
        Vector2 combinedForce = interactionForce + (steeringForce * arrivalWeight);

        force = combinedForce;
    }

    private void FixedUpdate()
    {
        _rb.AddForce(force, ForceMode2D.Force);
        _rb.linearVelocity *= Cluster.Friction; // Apply friction to slow down the particles
    }

    private float CalculateForce(float distance, float attractionValue)
    {
        float radius = Cluster.ParticleRadius;

        if (distance < radius)
        {
            return (distance / radius) - 1;
        }
        else if (radius < distance && distance < 1)
        {
            return attractionValue * (1 - Mathf.Abs(2 * distance - 1 - radius) / (1 - radius));
        }

        return 0;
    }

    public void SetType(int type) { SetType(ParticleTypeToEnum(type)); }
    public void SetType(ParticleTypes newType)
    {
        // Set type and also color based on type
        Color color = Cluster.GetColorByType((int)newType);
        GetComponent<SpriteRenderer>().color = color;

        Type = newType;
    }

    public static ParticleTypes ParticleTypeToEnum(int type)
    {
        return (ParticleTypes)(type % Cluster.Instance.NumberOfTypes);
    }

    private void OnDestroy()
    {
        if (Cluster == null) return;

        Cluster.Particles.Remove(this); // Remove this particle from the cluster's list
        Cluster.ParticleTypeCounts[Type]--; // Decrement the count for this type
    }
}
