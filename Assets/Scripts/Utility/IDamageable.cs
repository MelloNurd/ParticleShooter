using UnityEngine;

public interface IDamageable
{
    void Damage(float amount);
    void DamageOverTime(float amount, float duration);
    void HaltMovement(float duration);
}
