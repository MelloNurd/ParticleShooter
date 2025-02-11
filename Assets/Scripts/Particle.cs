using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public Vector3 Velocity { get; set; }
    public int Type { get; set; }

    private float dampening = 0.05f;
    private float friction = 0.85f;

    Vector3 direction;
    Vector3 totalForce;
    Vector3 acceleration;

    float distance;

    private void Update()
    {
        if (!ParticleManager.IsFinishedSpawning) return;

        direction = Vector3.zero;
        totalForce = Vector3.zero;
        acceleration = Vector3.zero;

        float[,] minDistances = ParticleManager.MinDistances;
        float[,] forces = ParticleManager.Forces;

        List<Particle> nearbyParticles = ParticleManager.Instance.GetNearbyParticles(this, 5); // Adjust search radius

        foreach (Particle particle in nearbyParticles)
        {
            if (particle == this) continue;

            direction = particle.transform.position - transform.position;
            float sqrDistance = direction.sqrMagnitude;
            direction.Normalize();

            if (sqrDistance < minDistances[Type, particle.Type])
            {
                Vector3 force = direction * -Mathf.Abs(forces[Type, particle.Type]) * 3;
                force *= Mathf.Abs(Mathf.Lerp(1, 0, Mathf.Sqrt(sqrDistance) / minDistances[Type, particle.Type]));
                force *= dampening;
                totalForce += force;
            }
        }

        acceleration += totalForce;
        Velocity += acceleration * Time.deltaTime;
        Velocity *= friction;
        transform.position += Velocity * Time.deltaTime;
    }
}
