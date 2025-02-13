using UnityEngine;

public class StandardBullet : Bullet
{
    public override void Initialize(Vector3 position, Quaternion rotation, float speed, float lifetime, float damage)
    {
        transform.position = position;
        transform.rotation = rotation;
        this.speed = speed;
        this.damage = damage;
    }

    protected override void ApplyDamage(IDamageable target)
    {
        target.Damage(damage);
    }
}
