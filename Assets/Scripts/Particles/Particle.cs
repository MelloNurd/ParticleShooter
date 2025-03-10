using UnityEngine;

namespace NaughtyAttributes
{
    public class Particle : MonoBehaviour
    {
        public Vector3 Position = Vector3.zero;

        public Vector3 Velocity;

        [ShowNativeProperty]
        public int Type { get; private set; }

        [ShowNativeProperty]
        public int Id { get; private set; }

        public Cluster ParentCluster;

        private Transform _transform;
        private SpriteRenderer _spriteRenderer;

        public float MinimalDistanceToPlayer { get; private set; }

        private Transform _playerTransform;

        private void Awake()
        {
            // Cache the transform and sprite renderer components
            _transform = GetComponent<Transform>();
            Position = _transform.position;

            _spriteRenderer = GetComponent<SpriteRenderer>();

            _playerTransform = ParticleManager.Instance.player.transform;
            MinimalDistanceToPlayer = float.MaxValue;
        }

        public void Initialize(int type, Cluster parentCluster)
        {
            Type = type;
            ParentCluster = parentCluster;

            // Set initial velocity 
            Velocity = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            );

            // Set the color of the particle based on its type
            SetColorByType();
        }

        public void Update()
        {
            float distanceToPlayer = Vector3.Distance(Position, _playerTransform.position);
            if (distanceToPlayer < MinimalDistanceToPlayer)
            {
                MinimalDistanceToPlayer = distanceToPlayer;
            }
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

        private void SetColorByType()
        {
            if (_spriteRenderer != null)
            {
                int totalTypes = ParticleManager.Instance.NumberOfTypes;
                Color color = GetColorForType(Type, totalTypes);
                _spriteRenderer.color = color;
            }
            else
            {
                Debug.LogWarning($"SpriteRenderer not found on Particle {Id}");
            }
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

        public void ApplyInternalForces(Cluster cluster)
        {
            if (this == null || _transform == null || cluster == null) return;

            Vector3 totalForce = Vector3.zero;

            foreach (Particle particle in cluster.Swarm)
            {
                if (particle == this || particle == null) continue;

                Vector3 direction = particle.Position - Position;

                float distance = direction.magnitude;
                direction.Normalize();

                // Repulsive forces
                if (distance < cluster.InternalMins[Type, particle.Type])
                {
                    Vector3 force = direction * Mathf.Abs(cluster.InternalForces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                    force *= Map(distance, 0, Mathf.Abs(cluster.InternalMins[Type, particle.Type]), 1, 0) * ParticleManager.Instance.Dampening;
                    totalForce += force;
                }

                // Attractive forces
                if (distance < cluster.InternalRadii[Type, particle.Type])
                {
                    Vector3 force = direction * cluster.InternalForces[Type, particle.Type];
                    force *= Map(distance, 0, cluster.InternalRadii[Type, particle.Type], 1, 0) * ParticleManager.Instance.Dampening;
                    totalForce += force;
                }
            }

            // Apply forces smoothly
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity * Time.deltaTime;
            Velocity *= ParticleManager.Instance.Friction;

            ConstrainPosition();

            if (_transform != null)
                _transform.position = Position;
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

                    Vector3 direction = particle.Position - Position;

                    float distance = direction.magnitude;
                    direction.Normalize();

                    // Repulsive forces
                    if (distance < cluster.ExternalMins[Type, particle.Type])
                    {
                        Vector3 force = direction * Mathf.Abs(cluster.ExternalForces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                        force *= Map(distance, 0, Mathf.Abs(cluster.ExternalMins[Type, particle.Type]), 1, 0) * ParticleManager.Instance.Dampening;
                        totalForce += force;
                    }

                    // Attractive forces 
                    if (distance < cluster.ExternalRadii[Type, particle.Type])
                    {
                        Vector3 force = direction * cluster.ExternalForces[Type, particle.Type];
                        force *= Map(distance, 0, cluster.ExternalRadii[Type, particle.Type], 1, 0) * ParticleManager.Instance.Dampening;
                        totalForce += force;
                    }
                }
            }

            // Apply forces smoothly
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity * Time.deltaTime;
            Velocity *= ParticleManager.Instance.Friction;

            ConstrainPosition();

            if (_transform != null)
                _transform.position = Position;
        }

        public void ApplyPlayerAttraction()
        {
            if (_transform == null || ParticleManager.Instance.player == null) return;

            Vector3 targetPosition = Position;

            // Attract to specific player points based on particle type
            if (Type % 2 == 0)
                targetPosition = ParticleManager.Instance.player.frontPoint.position;
            else if (Type % 2 == 1)
                targetPosition = ParticleManager.Instance.player.backPoint.position;
            else
                return;

            Vector3 direction = targetPosition - Position;

            float distance = direction.magnitude;
            direction.Normalize();

            float attractionStrength = ParticleManager.Instance.PlayerAttractionStrength;

            // Calculate attraction force
            Vector3 attractionForce = direction * attractionStrength * Map(distance, 0, 10f, 1f, 0f);

            // Apply force
            Velocity += attractionForce * Time.deltaTime;

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
            Position += Velocity * Time.deltaTime;
            transform.position = Position;
        }

        public void ApplyCohesion()
        {
            if (ParentCluster == null) return;

            Vector3 direction = ParentCluster.Center - Position;

            float distance = direction.magnitude;
            direction.Normalize();

            // Use the CohesionStrength from ParticleManager
            float cohesionStrength = ParticleManager.Instance.CohesionStrength;

            // Apply cohesion force
            Vector3 cohesionForce = direction * cohesionStrength;

            Velocity += cohesionForce * Time.deltaTime;
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
            if (Position.x - (-halfX) < borderThreshold)
                borderForce.x += repulsionStrength * (1 - (Position.x - (-halfX)) / borderThreshold);
            // Check right border
            if (halfX - Position.x < borderThreshold)
                borderForce.x -= repulsionStrength * (1 - (halfX - Position.x) / borderThreshold);
            // Check bottom border
            if (Position.y - (-halfY) < borderThreshold)
                borderForce.y += repulsionStrength * (1 - (Position.y - (-halfY)) / borderThreshold);
            // Check top border
            if (halfY - Position.y < borderThreshold)
                borderForce.y -= repulsionStrength * (1 - (halfY - Position.y) / borderThreshold);

            // Apply the repulsion force (scaled by deltaTime for consistency)
            Velocity += borderForce * Time.deltaTime;

            // --- Clamp the position so particles never go out-of-bounds ---
            float clampedX = Mathf.Clamp(Position.x, -halfX, halfX);
            float clampedY = Mathf.Clamp(Position.y, -halfY, halfY);
            Position = new Vector3(clampedX, clampedY, Position.z);

            // Reset velocity in a direction if the particle is at the boundary and moving further out
            if (clampedX == -halfX && Velocity.x < 0) Velocity.x = 0;
            if (clampedX == halfX && Velocity.x > 0) Velocity.x = 0;
            if (clampedY == -halfY && Velocity.y < 0) Velocity.y = 0;
            if (clampedY == halfY && Velocity.y > 0) Velocity.y = 0;
        }

    }
}
