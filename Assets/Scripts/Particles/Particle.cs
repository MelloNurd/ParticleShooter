using UnityEngine;

namespace NaughtyAttributes
{
    public class Particle : MonoBehaviour
    {
        [ShowNativeProperty]
        public Vector3 Position { get; set; } = Vector3.zero;

        [ShowNativeProperty]
        public Vector3 Velocity { get; set; }

        [ShowNativeProperty]
        public int Type { get; private set; }

        [ShowNativeProperty]
        public int Id { get; private set; }

        public Cluster ParentCluster;

        private Transform _transform;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            Position = _transform.position;

            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(int type, Cluster parentCluster)
        {
            Type = type;
            ParentCluster = parentCluster;

            // Set initial velocity (optional)
            Velocity = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            );

            SetColorByType();
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

                // Screen wrapping adjustments
                direction = ScreenWrapAdjustment(direction);

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

            WrapPosition();

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

                    // Screen wrapping adjustments
                    direction = ScreenWrapAdjustment(direction);

                    float distance = direction.magnitude;
                    direction.Normalize();

                    // Repulsive forces
                    if (distance < cluster.ExternalMins[Type, particle.Type])
                    {
                        Vector3 force = direction * Mathf.Abs(cluster.ExternalForces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                        force *= Map(distance, 0, Mathf.Abs(cluster.ExternalMins[Type, particle.Type]), 1, 0) * ParticleManager.Instance.Dampening;
                        totalForce += force;
                    }

                    // Attractive forces (if needed)
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

            WrapPosition();

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

            // Screen wrapping adjustments
            direction = ScreenWrapAdjustment(direction);

            float distance = direction.magnitude;
            direction.Normalize();

            float attractionStrength = ParticleManager.Instance.PlayerAttractionStrength;

            // Calculate attraction force
            Vector3 attractionForce = direction * attractionStrength * Map(distance, 0, 10f, 1f, 0f);

            // Apply force
            Velocity += attractionForce * Time.deltaTime;
        }

        private Vector3 ScreenWrapAdjustment(Vector3 direction)
        {
            if (direction.x > ParticleManager.Instance.HalfScreenSpace.x)
                direction.x -= ParticleManager.Instance.ScreenSpace.x;
            if (direction.x < -ParticleManager.Instance.HalfScreenSpace.x)
                direction.x += ParticleManager.Instance.ScreenSpace.x;
            if (direction.y > ParticleManager.Instance.HalfScreenSpace.y)
                direction.y -= ParticleManager.Instance.ScreenSpace.y;
            if (direction.y < -ParticleManager.Instance.HalfScreenSpace.y)
                direction.y += ParticleManager.Instance.ScreenSpace.y;

            return direction;
        }

        private float Map(float value, float inMin, float inMax, float outMin, float outMax)
        {
            value = Mathf.Clamp(value, inMin, inMax);
            return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
        }

        private void WrapPosition()
        {
            if (Position.x < -ParticleManager.Instance.HalfScreenSpace.x)
                Position = new Vector3(ParticleManager.Instance.HalfScreenSpace.x, Position.y, Position.z);
            else if (Position.x > ParticleManager.Instance.HalfScreenSpace.x)
                Position = new Vector3(-ParticleManager.Instance.HalfScreenSpace.x, Position.y, Position.z);

            if (Position.y < -ParticleManager.Instance.HalfScreenSpace.y)
                Position = new Vector3(Position.x, ParticleManager.Instance.HalfScreenSpace.y, Position.z);
            else if (Position.y > ParticleManager.Instance.HalfScreenSpace.y)
                Position = new Vector3(Position.x, -ParticleManager.Instance.HalfScreenSpace.y, Position.z);
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

            // Screen wrapping adjustments
            direction = ScreenWrapAdjustment(direction);

            float distance = direction.magnitude;
            direction.Normalize();

            // Use the CohesionStrength from ParticleManager
            float cohesionStrength = ParticleManager.Instance.CohesionStrength;

            // Apply cohesion force
            Vector3 cohesionForce = direction * cohesionStrength;

            Velocity += cohesionForce * Time.deltaTime;
        }
    }
}
