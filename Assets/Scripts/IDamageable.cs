using UnityEngine;

public interface IDamageable
{
    void Damage(float damage);
    void DamageOverTime(float damage, float duration);
}
