using NaughtyAttributes;
using UnityEngine;

public class ParticleHealth : MonoBehaviour, IDamageable
{
    public int health = 100;
    void Update()
    {
        // Check for damage and reduce health accordingly
        if (health <= 0)
        {
            ParticleJobManager jobManager = FindFirstObjectByType<ParticleJobManager>();
            jobManager.RequestParticleRemoval(GetComponent<Particle>());
            // Optionally, destroy the particle GameObject
            Destroy(gameObject);
        }
    }
    public void Damage(float amount)
    {
        health -= 100;
    }

    public void DamageOverTime(float amount, float duration)
    {
        health -= 100;
    }

    public void HaltMovement(float duration)
    {
        health -= 100;
    }

    public void ChainDamage(float amount, int remainingChains)
    {
        health -= 100;
    }
}
