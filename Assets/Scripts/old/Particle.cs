using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace NaughtyAttributes
{
    public class Particle : MonoBehaviour
    {
        public int Type { get; set; } // Determines the type of the particle, as well as the color
        public int Id { get; set; } // Can be used to single out specific particles

        private float _dampening = 0.05f;
        private float _friction = 0.85f;

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

            foreach (Particle particle in ParticleManager.Particles)
            {
                // Skip the current particle
                if (particle == this) continue;

                // Calculate the direction and squared distance to the other particle
                _direction = particle.transform.position - transform.position;

                // Wrapping world-space fixes for distance calculations
                if (_direction.x > ParticleManager.ScreenWidth)
                {
                    _direction -= new Vector3(2 * ParticleManager.ScreenWidth, 0, 0);
                }
                else if (_direction.x < -ParticleManager.ScreenWidth)
                {
                    _direction += new Vector3(2 * ParticleManager.ScreenWidth, 0, 0);
                }

                if (_direction.y > ParticleManager.ScreenHeight)
                {
                    _direction -= new Vector3(0, 2 * ParticleManager.ScreenHeight, 0);
                }
                else if (_direction.y < -ParticleManager.ScreenHeight)
                {
                    _direction += new Vector3(0, 2 * ParticleManager.ScreenHeight, 0);
                }

                _distance = _direction.magnitude;
                _direction.Normalize();

                // Calculate repulsive forces
                if (_distance < minDistances[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= Mathf.Abs(forces[Type, particle.Type]) * ParticleManager.Instance.RepulsionEffector;
                    force *= Map(_distance, 0, Mathf.Abs(minDistances[Type, particle.Type]), 1, 0);
                    force *= _dampening;
                    _totalForce += force;
                }

                // Calculate attractive forces
                if (_distance < ParticleManager.Radii[Type, particle.Type])
                {
                    Vector3 force = _direction;
                    force *= forces[Type, particle.Type];
                    force *= Map(_distance, 0, ParticleManager.Radii[Type, particle.Type], 1, 0);
                    force *= _dampening;
                    _totalForce += force;
                }
            }

            _acceleration += _totalForce;
            _velocity += _acceleration * Time.deltaTime;
            _velocity *= _friction;
            transform.position += _velocity * Time.deltaTime;

            // World-space wrapping
            //transform.position = new Vector3((transform.position.x + width) % width, (transform.position.y + height) % height, 0);
            if (transform.position.x < -ParticleManager.ScreenWidth)
            {
                transform.position = transform.position.WithX(ParticleManager.ScreenWidth);
            }
            if (transform.position.x > ParticleManager.ScreenWidth)
            {
                transform.position = transform.position.WithX(-ParticleManager.ScreenWidth);
            }

            if (transform.position.y < -ParticleManager.ScreenHeight)
            {
                transform.position = transform.position.WithY(ParticleManager.ScreenHeight);
            }
            if (transform.position.y > ParticleManager.ScreenHeight)
            {
                transform.position = transform.position.WithY(-ParticleManager.ScreenHeight);
            }
        }

        float Map(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
        }
    }
}