using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Entities.SystemBaseDelegates;
using UnityEngine.UIElements;

namespace NaughtyAttributes
{
    public class SimParticle : MonoBehaviour
    {
        [ShowNativeProperty] public Vector3 Position { get; set; } = Vector3.zero; // Storing this manually, despite having Transform, to make switching to DOD easier later
        [ShowNativeProperty] public Vector3 Velocity { get; set; } // Used to handle custom physics
        [ShowNativeProperty] public int Type { get; set; } // Determines the type of the particle, as well as the color
        [ShowNativeProperty] public int Id { get; set; } // Can be used to single out specific particles

        public SimCluster ParentCluster;

        private Transform _transform; // Caching the object's transform. Very slight performance gain.

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        public void ApplyInternalForces(SimCluster cluster)
        {
            Vector3 totalForce = Vector3.zero;
            Vector3 acceleration = Vector3.zero;

            foreach (SimParticle particle in cluster.Swarm)
            {
                // Skip the current particle
                if (particle == this) continue;

                // Calculate the direction and squared distance to the other particle
                Vector3 _direction = particle.Position - Position;

                // Wrapping world-space fixes for distance calculations
                if (_direction.x > SimParticleManager.Instance.HalfScreenSpace.x)
                {
                    _direction.x -= SimParticleManager.Instance.ScreenSpace.x;
                }
                if (_direction.x < -SimParticleManager.Instance.HalfScreenSpace.x)
                {
                    _direction.x += SimParticleManager.Instance.ScreenSpace.x;
                }

                if (_direction.y > SimParticleManager.Instance.HalfScreenSpace.y)
                {
                    _direction.y -= SimParticleManager.Instance.ScreenSpace.y;
                }
                if (_direction.y < -SimParticleManager.Instance.HalfScreenSpace.y)
                {
                    _direction.y += SimParticleManager.Instance.ScreenSpace.y;
                }

                float _distance = _direction.magnitude;
                _direction.Normalize();

                // Calculate repulsive forces
                if (_distance < cluster.InternalMins[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= Mathf.Abs(cluster.InternalForces[Type, particle.Type]) * SimParticleManager.Instance.RepulsionEffector;
                    force *= Map(_distance, 0, Mathf.Abs(cluster.InternalMins[Type, particle.Type]), 1, 0);
                    force *= SimParticleManager.Instance.Dampening;
                    totalForce += force;
                }

                // Calculate attractive forces
                if (_distance < cluster.InternalRadii[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= cluster.InternalForces[Type, particle.Type];
                    force *= Map(_distance, 0, cluster.InternalRadii[Type, particle.Type], 1, 0);
                    force *= SimParticleManager.Instance.Dampening;
                    totalForce += force;
                }
            }

            // Apply all forces after calculating with all particles
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity *= Time.deltaTime;
            Velocity *= SimParticleManager.Instance.Friction;

            // World-space wrapping (This can probably be cleaned up)
            if (Position.x < -SimParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(SimParticleManager.Instance.HalfScreenSpace.x);
            }
            if (Position.x > SimParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(-SimParticleManager.Instance.HalfScreenSpace.x);
            }

            if (Position.y < -SimParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(SimParticleManager.Instance.HalfScreenSpace.y);
            }
            if (Position.y > SimParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(-SimParticleManager.Instance.HalfScreenSpace.y);
            }

            _transform.position = Position;
        }

        public void ApplyExternalForces(SimCluster cluster)
        {
            Vector3 totalForce = Vector3.zero;

            

            // Apply all forces after calculating with all particles
            Velocity += totalForce * Time.deltaTime;
            Position += Velocity *= Time.deltaTime;
            Velocity *= SimParticleManager.Instance.Friction;

            // World-space wrapping (This can probably be cleaned up)
            if (Position.x < -SimParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(SimParticleManager.Instance.HalfScreenSpace.x);
            }
            if (Position.x > SimParticleManager.Instance.HalfScreenSpace.x)
            {
                Position = Position.WithX(-SimParticleManager.Instance.HalfScreenSpace.x);
            }

            if (Position.y < -SimParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(SimParticleManager.Instance.HalfScreenSpace.y);
            }
            if (Position.y > SimParticleManager.Instance.HalfScreenSpace.y)
            {
                Position = Position.WithY(-SimParticleManager.Instance.HalfScreenSpace.y);
            }

            _transform.position = Position;
        }

        private float Map(float value, float inMin, float inMax, float outMin, float outMax)
        {
            value = Mathf.Clamp(value, inMin, inMax);
            return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
        }
    }
}