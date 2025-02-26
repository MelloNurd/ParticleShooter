using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Entities.SystemBaseDelegates;
using UnityEngine.UIElements;

namespace NaughtyAttributes
{
    public class Particle : MonoBehaviour
    {
        [ShowNativeProperty] public Vector3 Position { get; set; } = Vector3.zero; // Storing this manually, despite having Transform, to make switching to DOD easier later
        [ShowNativeProperty] public Vector3 Velocity { get; set; } // Used to handle custom physics
        [ShowNativeProperty] public int Type { get; set; } // Determines the type of the particle, as well as the color
        [ShowNativeProperty] public int Id { get; set; } // Can be used to single out specific particles

        public Cluster ParentCluster;

        private Transform _transform; // Caching the object's transform. Very slight performance gain.

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        public void ApplyInternalForces(Cluster cluster)
        {
            if (this == null || _transform == null) return; // Check if the particle or its transform has been destroyed

            Vector3 totalForce = Vector3.zero;
            Vector3 acceleration = Vector3.zero;

            foreach (Particle particle in cluster.Swarm)
            {
                // Skip the current particle
                if (particle == this) continue;

                // Calculate the direction and squared distance to the other particle
                Vector3 _direction = particle.Position - Position;

                // Wrapping world-space fixes for distance calculations
                if (_direction.x > ParticleManager.Instance.HalfScreenSpace.x)
                {
                    _direction.x -= ParticleManager.Instance.ScreenSpace.x;
                }
                if (_direction.x < -ParticleManager.Instance.HalfScreenSpace.x)
                {
                    _direction.x += ParticleManager.Instance.ScreenSpace.x;
                }

                if (_direction.y > ParticleManager.Instance.HalfScreenSpace.y)
                {
                    _direction.y -= ParticleManager.Instance.ScreenSpace.y;
                }
                if (_direction.y < -ParticleManager.Instance.HalfScreenSpace.y)
                {
                    _direction.y += ParticleManager.Instance.ScreenSpace.y;
                }

                float _distance = _direction.magnitude;
                _direction.Normalize();

                // Calculate repulsive forces
                if (_distance < cluster.InternalMins[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= Mathf.Abs(cluster.InternalForces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                    force *= Map(_distance, 0, Mathf.Abs(cluster.InternalMins[Type, particle.Type]), 1, 0);
                    force *= ParticleManager.Instance.Dampening;
                    totalForce += force;
                }

                // Calculate attractive forces
                if (_distance < cluster.InternalRadii[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= cluster.InternalForces[Type, particle.Type];
                    force *= Map(_distance, 0, cluster.InternalRadii[Type, particle.Type], 1, 0);
                    force *= ParticleManager.Instance.Dampening;
                    totalForce += force;
                }
            }

            // Apply all forces after calculating with all particles
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity *= Time.deltaTime;
            Velocity *= ParticleManager.Instance.Friction;

            // World-space wrapping (This can probably be cleaned up)
            if (Position.x < -ParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(ParticleManager.Instance.HalfScreenSpace.x);
            }
            if (Position.x > ParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(-ParticleManager.Instance.HalfScreenSpace.x);
            }

            if (Position.y < -ParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(ParticleManager.Instance.HalfScreenSpace.y);
            }
            if (Position.y > ParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(-ParticleManager.Instance.HalfScreenSpace.y);
            }

            if (_transform != null) // Check if the transform is still valid
            {
                _transform.position = Position;
            }
        }

        public void ApplyExternalForces(Cluster cluster)
        {
            if (this == null || _transform == null) return; // Check if the particle or its transform has been destroyed

            Vector3 totalForce = Vector3.zero;

            foreach (Cluster otherColoy in ParticleManager.Clusters)
            {
                if (otherColoy == cluster) return;
                if (Vector2.Distance(cluster.Center, otherColoy.transform.position) > cluster.MaxExternalRadii) continue;

                foreach (Particle particle in otherColoy.Swarm)
                {
                    // Skip the current particle
                    if (particle == this) continue;

                    // Calculate the direction and squared distance to the other particle
                    Vector3 _direction = particle.Position - Position;

                    // Wrapping world-space fixes for distance calculations
                    if (_direction.x > ParticleManager.Instance.HalfScreenSpace.x)
                    {
                        _direction.x -= ParticleManager.Instance.ScreenSpace.x;
                    }
                    if (_direction.x < -ParticleManager.Instance.HalfScreenSpace.x)
                    {
                        _direction.x += ParticleManager.Instance.ScreenSpace.x;
                    }

                    if (_direction.y > ParticleManager.Instance.HalfScreenSpace.y)
                    {
                        _direction.y -= ParticleManager.Instance.ScreenSpace.y;
                    }
                    if (_direction.y < -ParticleManager.Instance.HalfScreenSpace.y)
                    {
                        _direction.y += ParticleManager.Instance.ScreenSpace.y;
                    }

                    float _distance = _direction.magnitude;
                    _direction.Normalize();

                    // Calculate repulsive forces
                    if (_distance < cluster.ExternalMins[Type, particle.Type])
                    {
                        Vector3 force = _direction;
                        force *= Mathf.Abs(cluster.ExternalForces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                        force *= Map(_distance, 0, Mathf.Abs(cluster.ExternalMins[Type, particle.Type]), 1, 0);
                        force *= ParticleManager.Instance.Dampening;
                        totalForce += force;
                    }

                    // Calculate attractive forces
                    if (_distance < cluster.ExternalRadii[Type, particle.Type])
                    {
                        Vector3 force = _direction;
                        force *= cluster.ExternalForces[Type, particle.Type];
                        force *= Map(_distance, 0, cluster.ExternalRadii[Type, particle.Type], 1, 0);
                        force *= ParticleManager.Instance.Dampening;
                        totalForce += force;
                    }
                }
            }

            // Apply all forces after calculating with all particles
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity *= Time.deltaTime;
            Velocity *= ParticleManager.Instance.Friction;

            // World-space wrapping (This can probably be cleaned up)
            if (Position.x < -ParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(ParticleManager.Instance.HalfScreenSpace.x);
            }
            if (Position.x > ParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(-ParticleManager.Instance.HalfScreenSpace.x);
            }

            if (Position.y < -ParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(ParticleManager.Instance.HalfScreenSpace.y);
            }
            if (Position.y > ParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(-ParticleManager.Instance.HalfScreenSpace.y);
            }

            if (_transform != null) // Check if the transform is still valid
            {
                _transform.position = Position;
            }
        }

        public void ApplyFoodForces(Cluster cluster)
        {
            if (this == null || _transform == null) return; // Check if the particle or its transform has been destroyed

            Vector3 totalForce = Vector3.zero;

            foreach (Particle food in ParticleManager.Food)
            {
                if (Vector2.Distance(cluster.Center, food.Position) > cluster.MaxExternalRadii) continue;

                // Calculate the direction and squared distance to the other particle
                Vector3 _direction = food.Position - Position;

                // Wrapping world-space fixes for distance calculations
                if (_direction.x > ParticleManager.Instance.HalfScreenSpace.x)
                {
                    _direction.x -= ParticleManager.Instance.ScreenSpace.x;
                }
                if (_direction.x < -ParticleManager.Instance.HalfScreenSpace.x)
                {
                    _direction.x += ParticleManager.Instance.ScreenSpace.x;
                }

                if (_direction.y > ParticleManager.Instance.HalfScreenSpace.y)
                {
                    _direction.y -= ParticleManager.Instance.ScreenSpace.y;
                }
                if (_direction.y < -ParticleManager.Instance.HalfScreenSpace.y)
                {
                    _direction.y += ParticleManager.Instance.ScreenSpace.y;
                }

                float _distance = _direction.magnitude;
                _direction.Normalize();

                // Calculate attractive forces
                if (_distance < cluster.ExternalRadii[Type, food.Type])
                {
                    Vector3 force = _direction;
                    force *= cluster.ExternalForces[Type, food.Type];
                    force *= Map(_distance, 0, cluster.ExternalRadii[Type, food.Type], 1, 0);
                    force *= ParticleManager.Instance.Dampening;
                    totalForce += force;
                }
            }

            // Apply all forces after calculating with all particles
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity *= Time.deltaTime;
            Velocity *= ParticleManager.Instance.Friction;

            // World-space wrapping (This can probably be cleaned up)
            if (Position.x < -ParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(ParticleManager.Instance.HalfScreenSpace.x);
            }
            if (Position.x > ParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(-ParticleManager.Instance.HalfScreenSpace.x);
            }

            if (Position.y < -ParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(ParticleManager.Instance.HalfScreenSpace.y);
            }
            if (Position.y > ParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(-ParticleManager.Instance.HalfScreenSpace.y);
            }

            if (_transform != null) // Check if the transform is still valid
            {
                _transform.position = Position;
            }
        }

        public void ApplyPlayerAttraction()
        {
            if (_transform == null || ParticleManager.Instance.player == null) return;

            Vector3 targetPosition = Position; // Default to current position

            // Decide which point to attract to based on Type
            if (Type%2 == 0)
            {
                // Attract to front point
                targetPosition = ParticleManager.Instance.player.frontPoint.position;
            }
            else if (Type%2 == 1)
            {
                // Attract to back point
                targetPosition = ParticleManager.Instance.player.backPoint.position;
            }
            else
            {
                // For other types, you can define behavior or return
                return;
            }

            // Calculate direction to the target point
            Vector3 direction = targetPosition - Position;

            // World-space wrapping adjustments, if necessary
            if (direction.x > ParticleManager.Instance.HalfScreenSpace.x)
            {
                direction.x -= ParticleManager.Instance.ScreenSpace.x;
            }
            if (direction.x < -ParticleManager.Instance.HalfScreenSpace.x)
            {
                direction.x += ParticleManager.Instance.ScreenSpace.x;
            }

            if (direction.y > ParticleManager.Instance.HalfScreenSpace.y)
            {
                direction.y -= ParticleManager.Instance.ScreenSpace.y;
            }
            if (direction.y < -ParticleManager.Instance.HalfScreenSpace.y)
            {
                direction.y += ParticleManager.Instance.ScreenSpace.y;
            }

            // Normalize direction and compute force
            float distance = direction.magnitude;
            direction.Normalize();

            // Define the attraction force strength
            float attractionStrength = ParticleManager.Instance.PlayerAttractionStrength;

            // Calculate the force (you may adjust the formula as needed)
            Vector3 attractionForce = direction * attractionStrength * Map(distance, 0, 10f, 1f, 0f);

            // Apply the force
            Velocity += attractionForce * Time.deltaTime;
        }

        private float Map(float value, float inMin, float inMax, float outMin, float outMax)
        {
            value = Mathf.Clamp(value, inMin, inMax);
            return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
        }

        [BurstCompile]
        public struct ParticleJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<ParticleData> Particles;
            public NativeArray<ParticleData> UpdatedParticles;
            public float DeltaTime;

            public void Execute(int index)
            {
                ParticleData particle = Particles[index];
                // Apply forces and update position here
                particle.Position += particle.Velocity * DeltaTime;
                UpdatedParticles[index] = particle;
            }
        }

        [BurstCompile]
        public struct ParticleData
        {
            public float3 Position;
            public float3 Velocity;
            public int Type;
            public int Id;
        }
    }
}
