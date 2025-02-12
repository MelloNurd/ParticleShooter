using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    // Initialize method to be overridden by child classes
    public abstract void Initialize(Vector3 position, Quaternion rotation, float speed, float lifetime);

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
        // If bullet hits something, return to pool
        if (other.CompareTag("Enemy"))
        {
            HandleCollision(other); // Handle specific behavior on collision
        }
    }

    // Handle specific collision behavior for different bullet types
    public abstract void HandleCollision(Collider2D other);

    private void OnDisable()
    {
        // Cancel Invoke if the bullet is disabled early
        CancelInvoke();
    }
}
