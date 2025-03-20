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

    public float MinimalDistanceToPlayer { get; private set; }

    private Transform _playerTransform;

    private bool _isLaunched = false;
    private float _maxTravelDistance;
    private Vector3 _launchPosition;
    public bool IsProjectile { get; set; } = false;
    private Vector3 _launchStartPosition;


    public void SetColor() => SetColor(stats.GetColorByType());
    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    private Color GetColorForType(int type, int totalTypes)
    {
        if (totalTypes <= 1)
        {
            // Default to red if there's only one type
            return Color.HSVToRGB(0f, 1f, 1f);
        }

        float hue = (float)type / (totalTypes - 1);

        Color color = Color.HSVToRGB(hue, 1f, 1f);

        return color;
    }

    private void Awake()
    {
        // Cache the transform and sprite renderer components
        _transform = GetComponent<Transform>();
        position = _transform.position;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        _playerTransform = ParticleManager.Instance.player.transform;
        MinimalDistanceToPlayer = float.MaxValue;
    }

    public ParticleType Initialize(Cluster parentCluster, int id)
    {
        ParentCluster = parentCluster;

        // Set initial velocity 
        velocity = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            0
        );

        ParticleType type = (ParticleType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ParticleType)).Length); // Temp; Random type
        stats = new ParticleStats(this, id, type);

        SetColor();

        return type;
    }

    private void Update()
    {
        if (_isLaunched)
        {
            float traveledDistance = Vector3.Distance(position, _launchStartPosition);
            if (traveledDistance >= _maxTravelDistance)
            {
                _isLaunched = false;
                velocity = Vector3.zero;

                // Remove from the previous cluster
                if (ParentCluster != null)
                {
                    ParentCluster.Swarm.Remove(this);
                    ParentCluster = null;
                }

                // Set neutral color
                SetColor(Color.gray);
            }
        }
        else
        {
            // Original behavior for active particles
            ApplyInternalForces(ParentCluster);
            ApplyExternalForces(ParentCluster);
            ApplyPlayerAttraction();
            ApplyCohesion();
        }

        ConstrainPosition();
        position += velocity * Time.deltaTime;
        _transform.position = position;
    }


    private void OnDestroy()
    {
        // Report minimal distance to parent cluster
        if (ParentCluster != null)
        {
            ParentCluster.ReportParticleProximity(MinimalDistanceToPlayer);
        }

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

        ConstrainPosition();

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

        ConstrainPosition();

        if (_transform != null)
            _transform.position = position;
    }

    public void ApplyPlayerAttraction()
    {
        if (_transform == null || ParticleManager.Instance.player == null) return;

        Vector3 targetPosition = position;

        // Attract to specific player points based on particle type
        if (stats.TypeInt % 2 == 0)
            targetPosition = ParticleManager.Instance.player.frontPoint.position;
        else if (stats.TypeInt % 2 == 1)
            targetPosition = ParticleManager.Instance.player.backPoint.position;
        else
            return;

        Vector3 direction = targetPosition - position;

        float distance = direction.magnitude;
        direction.Normalize();

        float attractionStrength = ParticleManager.Instance.PlayerAttractionStrength;

        // Calculate attraction force
        Vector3 attractionForce = direction * attractionStrength * Map(distance, 0, 10f, 1f, 0f);

        // Apply force
        velocity += attractionForce * Time.deltaTime;

        ConstrainPosition();
    }

    private float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        value = Mathf.Clamp(value, inMin, inMax);
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Notify the parent cluster of the hit
            ParentCluster.HitsToPlayer++;

            // Destroy the particle
            Destroy(gameObject);
        }
    }

    public void UpdatePosition()
    {
        // Update the particle's position based on its velocity and other forces
        position += velocity * Time.deltaTime;
        transform.position = position;
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

    // This is used to keep the particles within the defined game-boundaries
    private void ConstrainPosition()
    {
        float halfX = ParticleManager.Instance.HalfScreenSpace.x;
        float halfY = ParticleManager.Instance.HalfScreenSpace.y;

        // --- Optional: apply a border repulsion force if too close to the edge ---
        float borderThreshold = 1.0f;     // distance from the border at which to start repelling
        float repulsionStrength = 10.0f;    // adjust this to change how strongly particles are pushed inward
        Vector3 borderForce = Vector3.zero;

        // Check left border
        if (position.x - (-halfX) < borderThreshold)
            borderForce.x += repulsionStrength * (1 - (position.x - (-halfX)) / borderThreshold);
        // Check right border
        if (halfX - position.x < borderThreshold)
            borderForce.x -= repulsionStrength * (1 - (halfX - position.x) / borderThreshold);
        // Check bottom border
        if (position.y - (-halfY) < borderThreshold)
            borderForce.y += repulsionStrength * (1 - (position.y - (-halfY)) / borderThreshold);
        // Check top border
        if (halfY - position.y < borderThreshold)
            borderForce.y -= repulsionStrength * (1 - (halfY - position.y) / borderThreshold);

        // Apply the repulsion force (scaled by deltaTime for consistency)
        velocity += borderForce * Time.deltaTime;

        // --- Clamp the position so particles never go out-of-bounds ---
        float clampedX = Mathf.Clamp(position.x, -halfX, halfX);
        float clampedY = Mathf.Clamp(position.y, -halfY, halfY);
        position = new Vector3(clampedX, clampedY, position.z);

        // Reset velocity in a direction if the particle is at the boundary and moving further out
        if (clampedX == -halfX && velocity.x < 0) velocity.x = 0;
        if (clampedX == halfX && velocity.x > 0) velocity.x = 0;
        if (clampedY == -halfY && velocity.y < 0) velocity.y = 0;
        if (clampedY == halfY && velocity.y > 0) velocity.y = 0;
    }

    public void Launch(Vector3 direction, float speed, float maxDistance)
    {
        _isLaunched = true;
        _maxTravelDistance = maxDistance;
        _launchStartPosition = position;
        velocity = direction.normalized * speed;
    }

}