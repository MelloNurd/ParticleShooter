using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    private Transform firePoint;
    public PoolType bulletPoolType = PoolType.Bullets;
    private float bulletSpeed = 20f;
    private float fireRate = 1f;
    private float nextFireTime = 0f;

    private void Start()
    {
        //Finds the end of the blaster
        firePoint = transform.Find("Chamber");
    }

    private void Update()
    {
        // Fire a bullet when the player clicks
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        // Spawn a bullet from the pool
        GameObject bullet = ObjectPoolManager.SpawnObject(bulletPrefab, firePoint.position, firePoint.rotation, bulletPoolType);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * bulletSpeed;
        // Ensure the bullet is active
        bullet.SetActive(true);
    }
}
