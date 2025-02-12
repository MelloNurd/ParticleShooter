using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;

    private void OnEnable()
    {
        // Automatically return the bullet to the pool after "lifetime" seconds
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If bullet hits something, return to pool
        if (other.CompareTag("Enemy"))
        {
            // You can add damage logic here
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }

    private void OnDisable()
    {
        // Cancel Invoke if the bullet is disabled early
        CancelInvoke();
    }
}
