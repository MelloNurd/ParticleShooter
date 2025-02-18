using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace NaughtyAttributes
{
    public class Particle : MonoBehaviour
    {
        public int Type { get; set; } // Determines the type of the particle, as well as the color
        public int Id { get; set; } // Can be used to single out specific particles

        private Vector3 _velocity;
        private Vector3 _direction;
        private Vector3 _totalForce;
        private Vector3 _acceleration;

        private float _distance;

        private void Update()
        {
            // If the spawner has not completed spawning, do nothing
            if (!ParticleManager.IsFinishedSpawning) return;

            _totalForce = Vector3.zero;
            _acceleration = Vector3.zero;

            float[,] minDistances = ParticleManager.MinDistances;
            float[,] forces = ParticleManager.Forces;

            bool printValues = false;

            if(Input.GetKeyDown(KeyCode.M))
            {
                printValues = true;
            }

            foreach (Particle particle in ParticleManager.Particles)
            {
                // Skip the current particle
                if (particle == this) continue;

                // Calculate the direction and squared distance to the other particle
                _direction = particle.transform.position - transform.position;

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

                _distance = _direction.magnitude;
                _direction.Normalize();

                // Calculate repulsive forces
                if (_distance < minDistances[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= Mathf.Abs(forces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                    force *= Map(_distance, 0, Mathf.Abs(minDistances[Type, particle.Type]), 1, 0);
                    force *= ParticleManager.Instance.Dampening;
                    _totalForce += force;
                }

                // Calculate attractive forces
                if (_distance < ParticleManager.Radii[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= forces[Type, particle.Type];
                    force *= Map(_distance, 0, ParticleManager.Radii[Type, particle.Type], 1, 0);
                    force *= ParticleManager.Instance.Dampening;
                    _totalForce += force;
                }
            }

            printValues = false;

            // Apply all forces after calculating with all particles
            _acceleration += _totalForce;
            _velocity += _acceleration * Time.deltaTime;
            _velocity *= ParticleManager.Instance.Friction;
            transform.position += _velocity * Time.deltaTime;

            // World-space wrapping (This can probably be cleaned up)
            if (transform.position.x < -ParticleManager.Instance.HalfScreenSpace.x)
            {
                transform.position = transform.position.WithX(ParticleManager.Instance.HalfScreenSpace.x);
            }
            if (transform.position.x > ParticleManager.Instance.HalfScreenSpace.x)
            {
                transform.position = transform.position.WithX(-ParticleManager.Instance.HalfScreenSpace.x);
            }

            if (transform.position.y < -ParticleManager.Instance.HalfScreenSpace.y)
            {
                transform.position = transform.position.WithY(ParticleManager.Instance.HalfScreenSpace.y);
            }
            if (transform.position.y > ParticleManager.Instance.HalfScreenSpace.y)
            {
                transform.position = transform.position.WithY(-ParticleManager.Instance.HalfScreenSpace.y);
            }
        }

        float Map(float value, float inMin, float inMax, float outMin, float outMax)
   {
       value = Mathf.Clamp(value, inMin, inMax);
       return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
   }
    }
}