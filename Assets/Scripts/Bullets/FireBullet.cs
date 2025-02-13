using UnityEngine;

public class FireBullet : Bullet
{
    public float burnDamage = 2f;
    public float burnDuration = 5f;

    public override void Initialize(Vector3 position, Quaternion rotation, float speed, float lifetime, float damage)
    {
        transform.position = position;
        transform.rotation = rotation;
        this.speed = speed;
        this.damage = damage;
    }

    protected override void ApplyDamage(IDamageable target)
    {
        target.Damage(damage); // Initial hit
        target.DamageOverTime(burnDamage, burnDuration); // Apply burning effect
    }
}

