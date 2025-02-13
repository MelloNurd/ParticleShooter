using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable
{
    public float maxHealth = 50f;
    private float health;
    private bool isHalted = false;

    private Slider healthSlider;
    private RectTransform sliderTransform;
    public Vector3 sliderOffset = new Vector3(0, 1.5f, 0); // Adjust height above enemy

    public float moveSpeed = 2f;
    public float moveDistance = 2f;
    private bool isMovingUp = true;
    private Vector3 startPosition;

    private Camera mainCamera;

    private void Awake()
    {
        health = maxHealth;
        mainCamera = Camera.main;

        // Find the slider in children
        healthSlider = GetComponentInChildren<Slider>();
        if (healthSlider != null)
        {
            sliderTransform = healthSlider.GetComponent<RectTransform>();
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
        else
        {
            Debug.LogError("Health slider not found on Dummy!");
        }

        startPosition = transform.position;
    }

    private void Update()
    {
        if (healthSlider != null && mainCamera != null)
        {
            // Convert world position to screen position
            Vector3 worldPosition = transform.position + sliderOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // Update the slider position
            sliderTransform.position = screenPosition;
        }

        if (!isHalted)
        {
            MoveUpAndDown();
        }
    }

    private void MoveUpAndDown()
    {
        float direction = isMovingUp ? 1 : -1;
        transform.position += Vector3.up * direction * moveSpeed * Time.deltaTime;

        // Reverse direction when reaching max distance
        if (Vector3.Distance(startPosition, transform.position) >= moveDistance)
        {
            isMovingUp = !isMovingUp;
        }
    }

    public void Damage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();

        UpdateHealthUI();

        Debug.Log($"Enemy took {amount} damage. Health left: {health}");
        if (health <= 0)
        {
            Respawn();
        }
    }

    public void DamageOverTime(float amount, float duration)
    {
        StartCoroutine(StartDamageOverTime(amount, duration));
    }

    public void HaltMovement(float duration)
    {
        if (!isHalted) StartCoroutine(HaltCoroutine(duration));
    }

    private IEnumerator HaltCoroutine(float duration)
    {
        isHalted = true;
        yield return new WaitForSeconds(duration);
        isHalted = false;
    }

    private void Respawn()
    {
        health = maxHealth;
        UpdateHealthUI();
        Debug.Log("Enemy respawned!");
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    private IEnumerator StartDamageOverTime(float damagePerSecond, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            health -= damagePerSecond;
            health = Mathf.Max(health, 0); // Prevent negative health

            UpdateHealthUI();

            Debug.Log($"Enemy burning: {damagePerSecond} damage. Health left: {health}");
            if (health <= 0)
            {
                Respawn();
                yield break; // Stop the coroutine if the enemy "dies"
            }

            elapsedTime += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    private void Die()
    {
        Respawn();
    }
}
