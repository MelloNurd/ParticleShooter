using UnityEngine;

public class FireBullet : Bullet
{
    public override void Initialize(Vector3 position, Quaternion rotation, float speed, float lifetime)
    {
        transform.position = position;
        transform.rotation = rotation;
        this.speed = speed;
        this.lifetime = lifetime;
    }

    public override void HandleCollision(Collider2D other)
    {
        // Standard bullet behavior on collision
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy with fire bullet!");
            // Add damage logic here, e.g., other.GetComponent<Enemy>().TakeDamage(damage);
            ReturnToPool();
        }
    }
}
