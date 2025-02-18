using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHealth : MonoBehaviour, IDamageable
{
    public float health = 10f;

    public void Damage(float amount)
    {
        health -= amount;
        Debug.Log($"Enemy took {amount} damage. Health left: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    public void DamageOverTime(float amount, float duration)
    {
        StartCoroutine(StartDamageOverTime(amount, duration));
    }

    private IEnumerator StartDamageOverTime(float damagePerSecond, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            health -= damagePerSecond;
            health = Mathf.Max(health, 0); // Prevent negative health

            Debug.Log($"Enemy burning: {damagePerSecond} damage. Health left: {health}");

            if (health <= 0)
            {
                Die();
                yield break; // Stop the coroutine if the enemy "dies"
            }

            elapsedTime += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    public void HaltMovement(float duration)
    {
        health -= 100;
    }

    public void ChainDamage(float amount, int remainingChains)
    {
        Damage(amount);

        Debug.Log($"Enemy chain lightning damage: {amount} damage. Health left: {health}");

        if (health <= 0)
        {
            Die();
        }

        if (remainingChains > 0)
        {
            StartCoroutine(ChainLightning(amount * 0.8f, remainingChains - 1));
        }
    }

    private IEnumerator ChainLightning(float damage, int remainingChains)
    {
        yield return new WaitForSeconds(0.2f); // Short delay before chaining

        Collider2D[] nearbyParticles = Physics2D.OverlapCircleAll(transform.position, 3f);
        List<Particle> validTargets = new List<Particle>();

        foreach (Collider2D col in nearbyParticles)
        {
            Particle particle = col.GetComponent<Particle>();
            if (particle != null && particle != this.GetComponent<Particle>()) // Avoid hitting itself
            {
                validTargets.Add(particle);
            }
        }

        if (validTargets.Count > 0)
        {
            Particle nextTarget = validTargets[Random.Range(0, validTargets.Count)];
            ParticleHealth particleHealth = nextTarget.GetComponent<ParticleHealth>();
            if (particleHealth != null)
            {
                particleHealth.ChainDamage(damage, remainingChains);
            }
        }
    }

    private void Die()
    {
        ParticleJobManager jobManager = FindFirstObjectByType<ParticleJobManager>();
        jobManager.RequestParticleRemoval(GetComponent<Particle>());
        Destroy(gameObject);
    }
}
