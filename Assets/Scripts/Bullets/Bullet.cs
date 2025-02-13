using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float damage = 10f;

    // Initialize method to be overridden by child classes
    public abstract void Initialize(Vector3 position, Quaternion rotation, float speed, float lifetime, float damage);

    private void OnEnable()
    {
        // Automatically return the bullet to the pool after "lifetime" seconds
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void Update()
    {
        // Common movement logic, overridden if needed in child classes
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    protected void ReturnToPool()
    {
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            ApplyDamage(damageable);
            ReturnToPool();
        }
    }
    protected abstract void ApplyDamage(IDamageable target);

    private void OnDisable()
    {
        // Cancel Invoke if the bullet is disabled early
        CancelInvoke();
    }
}
