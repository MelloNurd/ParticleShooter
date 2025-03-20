using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum ParticleType
{
    Neutral,    // White
    Fire,       // Orange
    Ice,        // Blue
    Electric,   // Yellow
    Attack,     // Red
    Defense,    // Green
    Speed       // Pink
}

[Serializable]
public class ParticleStats
{
    public struct ElementalResistances
    {
        public float fire;
        public float ice;
        public float electric;
    }

    public int Id { get; private set; } // Unique identifier for the particle
    public ParticleType Type { get; private set; } // Type of the particle (e.g., Fire, Ice, etc.)
    public int TypeInt; // Caching this for performance

    public float health;
    public float speed;
    public ElementalResistances resistances;

    private Particle _particle; // Reference to the attached Particle

    public ParticleStats(Particle particle, int id, ParticleType type)
    {
        _particle = particle;
        Id = id;
        Type = type;
        TypeInt = (int)type; // Cache the type ID for performance
    }

    public void Damage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            health = 0; // Prevent negative health
            Die();
        }
    }

    public async void DamageOverTime(float damagePerSecond, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Damage(damagePerSecond);
            if (health <= 0) break; // Have to check this separately, as it would continue running even with Die() being called in Damage().

            elapsedTime += 1f;

            await UniTask.Delay(1000);
        }
    }

    public void Die()
    {
        UnityEngine.Object.Destroy(_particle.gameObject);
    }

    public Color GetColorByType()
    {
        return Type switch
        { 
            ParticleType.Fire => new Color(1f, 0.5f, 0f),
            ParticleType.Ice => Color.blue,
            ParticleType.Electric => Color.yellow,
            ParticleType.Attack => Color.red,
            ParticleType.Defense => Color.green,
            ParticleType.Speed => Color.magenta,
            ParticleType.Neutral => Color.white, // Neutral
            _ => Color.gray, // Default, should not ever happen
        };
    }
}
