using UnityEngine;

public class IceBullet : Bullet
{
    public float haltDuration = 1f;
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
        target.HaltMovement(haltDuration);
    }
}