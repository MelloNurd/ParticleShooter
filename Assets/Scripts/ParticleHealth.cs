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
            //Die();
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
                //Die();
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
        health -= amount;

        Debug.Log($"Enemy chain lightning damage: {amount} damage. Health left: {health}");

        if (remainingChains > 0)
        {
            StartCoroutine(ChainLightning(amount * 0.9f, remainingChains - 1));
        }

        if (health <= 0 && remainingChains == 0)
        {
            //Die();
        }
    }

    private IEnumerator ChainLightning(float damage, int remainingChains)
    {
        yield return new WaitForSeconds(0.2f); // Short delay before chaining

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 3f);
        Debug.Log($"Found {nearbyColliders.Length} colliders within range.");

        List<ParticleHealth> validTargets = new List<ParticleHealth>();

        foreach (Collider2D col in nearbyColliders)
        {
            ParticleHealth particleHealth = col.GetComponent<ParticleHealth>();
            if (particleHealth != null && particleHealth != this)
            {
                validTargets.Add(particleHealth);
                Debug.Log($"Added {col.gameObject.name} to validTargets.");
            }
            else
            {
                Debug.Log($"Skipped {col.gameObject.name} - ParticleHealth component not found or self-targeting.");
            }
        }

        Debug.Log($"Total valid targets: {validTargets.Count}");

        if (validTargets.Count > 0)
        {
            ParticleHealth nextTarget = validTargets[Random.Range(0, validTargets.Count)];
            if (nextTarget != null)
            {
                Debug.Log($"Chained to next target: {nextTarget.gameObject.name}");
                nextTarget.ChainDamage(damage, remainingChains);
            }
        }
        else
        {
            Debug.Log("No valid targets found for chain lightning.");
        }
    }

    //private void Die()
    //{
    //    Particle particle = GetComponent<Particle>();
    //    if (particle != null)
    //    {
    //        ParticleJobManager jobManager = FindFirstObjectByType<ParticleJobManager>();
    //        if (jobManager != null)
    //        {
    //            jobManager.RequestParticleRemoval(particle);
    //        }
    //        else
    //        {
    //            Debug.LogWarning("ParticleJobManager not found in the scene.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Particle component not found on the game object.");
    //    }

    //    Destroy(gameObject);
    //}
}
