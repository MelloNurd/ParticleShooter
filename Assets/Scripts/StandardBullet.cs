using UnityEngine;

public class StandardBullet : Bullet
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
        // Standard bullet behavior
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy with standard bullet!");
            // Add damage logic here;
            ReturnToPool();
        }
    }
}
