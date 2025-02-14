using UnityEngine;

public class ElectricBullet : Bullet
{
    public int maxChains = 5;
    public float chainRadius = 3f;
    public float chainDamageMultiplier = 0.8f; // Each chain does 80% of previous damage

    public override void Initialize(Vector3 position, Quaternion rotation, float speed, float lifetime, float damage)
    {
        transform.position = position;
        transform.rotation = rotation;
        this.speed = speed;
        this.damage = damage;
    }

    protected override void ApplyDamage(IDamageable target)
    {
        target.ChainDamage(damage, maxChains);
    }
}