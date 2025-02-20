using NUnit.Framework;
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

        private Transform _transform; // Caching the object's transform. Very slight performance gain.

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        public void ApplyInternalForces(Cluster cluster)
        {
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

            _transform.position = Position;
        }

        public void ApplyExternalForces(Cluster cluster)
        {
            Vector3 totalForce = Vector3.zero;

            foreach(Cluster otherColoy in ParticleManager.Clusters)
            {
                if (otherColoy == cluster) return;

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

            _transform.position = Position;
        }

        public void ApplyFoodForces(Cluster cluster)
        {
            Vector3 totalForce = Vector3.zero;

            foreach (Particle food in ParticleManager.Food)
            {
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

            _transform.position = Position;
        }

        // private void Update()
        // {
            //// If the spawner has not completed spawning, do nothing
            //if (!ParticleManager.IsFinishedSpawning) return;

            //Vector3 _totalForce = Vector3.zero;
            //Vector3 _acceleration = Vector3.zero;

            //float[,] minDistances = ParticleManager.MinDistances;
            //float[,] forces = ParticleManager.Forces;

            //foreach (Particle particle in ParticleManager.Particles)
            //{
            //    // Skip the current particle
            //    if (particle == this) continue;

            //    // Calculate the direction and squared distance to the other particle
            //    Vector3 _direction = particle.Position - Position;

            //    // Wrapping world-space fixes for distance calculations
            //    if (_direction.x > ParticleManager.Instance.HalfScreenSpace.x)
            //    {
            //        _direction.x -= ParticleManager.Instance.ScreenSpace.x;
            //    }
            //    if (_direction.x < -ParticleManager.Instance.HalfScreenSpace.x)
            //    {
            //        _direction.x += ParticleManager.Instance.ScreenSpace.x;
            //    }

            //    if (_direction.y > ParticleManager.Instance.HalfScreenSpace.y)
            //    {
            //        _direction.y -= ParticleManager.Instance.ScreenSpace.y;
            //    }
            //    if (_direction.y < -ParticleManager.Instance.HalfScreenSpace.y)
            //    {
            //        _direction.y += ParticleManager.Instance.ScreenSpace.y;
            //    }

            //    float _distance = _direction.magnitude;
            //    _direction.Normalize();

            //    // Calculate repulsive forces
            //    if (_distance < minDistances[Type, particle.Type])
            //    {
            //        Vector3 force = _direction;
            //        force *= Mathf.Abs(forces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
            //        force *= Map(_distance, 0, Mathf.Abs(minDistances[Type, particle.Type]), 1, 0);
            //        force *= ParticleManager.Instance.Dampening;
            //        _totalForce += force;
            //    }

            //    // Calculate attractive forces
            //    if (_distance < ParticleManager.Radii[Type, particle.Type])
            //    {
            //        Vector3 force = _direction;
            //        force *= forces[Type, particle.Type];
            //        force *= Map(_distance, 0, ParticleManager.Radii[Type, particle.Type], 1, 0);
            //        force *= ParticleManager.Instance.Dampening;
            //        _totalForce += force;
            //    }
            //}

            //// Apply all forces after calculating with all particles
            //_acceleration += _totalForce;
            //Velocity += _acceleration * Time.deltaTime;
            //Velocity *= ParticleManager.Instance.Friction;
            //Position += Velocity *= Time.deltaTime;

            //// World-space wrapping (This can probably be cleaned up)
            //if (Position.x < -ParticleManager.Instance.HalfScreenSpace.x)
            //{
            //    Position = Position.WithX(ParticleManager.Instance.HalfScreenSpace.x);
            //}
            //if (Position.x > ParticleManager.Instance.HalfScreenSpace.x)
            //{
            //    Position = Position.WithX(-ParticleManager.Instance.HalfScreenSpace.x);
            //}

            //if (Position.y < -ParticleManager.Instance.HalfScreenSpace.y)
            //{
            //    Position = Position.WithY(ParticleManager.Instance.HalfScreenSpace.y);
            //}
            //if (Position.y > ParticleManager.Instance.HalfScreenSpace.y)
            //{
            //    Position = Position.WithY(-ParticleManager.Instance.HalfScreenSpace.y);
            //}

            //_transform.position = Position;
        // }

        private float Map(float value, float inMin, float inMax, float outMin, float outMax)
        {
            value = Mathf.Clamp(value, inMin, inMax);
            return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
        }
    }
}