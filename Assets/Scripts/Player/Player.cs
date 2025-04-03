using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [BoxGroup("Game Settings")] public float minInteractionRadius = 20f;
    [BoxGroup("Game Settings")] [Tooltip("This only is visible in Scene view.")] [SerializeField]
    private bool drawPlayerInteractionRadius = false;

    [BoxGroup("Movement Settings")] public float rotationSpeed = 0.75f;
    [BoxGroup("Movement Settings")] public float movementSpeed = 5f;
    [BoxGroup("Movement Settings")] public float boostMultiplier = 1.75f;  // Boost increases speed
    [BoxGroup("Movement Settings")] public float maxBoost = 100f; // Total amount of boost the player has
    [BoxGroup("Movement Settings")] public float boostUsageRate = 20f; // How fast the boost depletes
    [BoxGroup("Movement Settings")] public float boostRechargeRate = 2; // How fast the boost recharges
    private float boostAmount; // Handles the current amount of boost the player has (how filled the bar is)

    [ShowNativeProperty] public int CurrentZone => Zones.GetCurrentZone(transform.position);

    private float vertMovement;
    private float horzMovement;

    private bool isBoosting;
    [BoxGroup("Thruster Settings")] public bool exhaustActive;

    [BoxGroup("Health Settings")] public float currentHealth = 100;
    [BoxGroup("Health Settings")] public float maxHealth = 100;
    [BoxGroup("Health Settings")] public float healthRegenRate = 1f;

    private GameObject standardBlaster;
    private GameObject fireBlaster;
    private GameObject iceBlaster;
    private GameObject electricBlaster;

    private GameObject leftExhhaust;
    private GameObject rightExhaust;

    private Slider boostSlider;
    private Slider healthSlider;
    private Slider energySlider;

    [HideInInspector] public UnityEvent onDeath;

    [BoxGroup("Overshield Settings")] public float invincibilityTime = .1f;
    private float currentInvincibilityTime = 2f;
    private GameObject overShield;

    private Rigidbody2D rb;

    [BoxGroup("Energy Settings")] public float maxEnergy = 100f;
    [BoxGroup("Energy Settings")] public float currentEnergy = 100f;
    [BoxGroup("Energy Settings")] public float movementEnergyCost = 1f;
    [BoxGroup("Energy Settings")] public bool consumeEnergy = true;

    private void Awake()
    {
        // Singleton Implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boostAmount = maxBoost;

        // Get the blaster objects
        standardBlaster = transform.Find("Weapon").gameObject;

        boostSlider = GameObject.Find("PlayerBoost").GetComponent<Slider>();
        healthSlider = GameObject.Find("PlayerHealth").GetComponent<Slider>();
        energySlider = GameObject.Find("PlayerEnergy").GetComponent<Slider>();

        leftExhhaust = transform.Find("LeftThruster").transform.Find("LeftExhaust").gameObject;
        rightExhaust = transform.Find("RightThruster").transform.Find("RightExhaust").gameObject;
        leftExhhaust.SetActive(false);
        rightExhaust.SetActive(false);

        overShield = transform.Find("OverShield").gameObject;
        overShield.SetActive(true);

        if (!consumeEnergy)
        {
            movementEnergyCost = 0;
        }
    }

    void Update()
    {
        // Movement input
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");

        // Boosting input
        isBoosting = Input.GetKey(KeyCode.LeftShift) && boostAmount > 0 && vertMovement != 0;
        boostSlider.value = boostAmount / maxBoost;

        RegenHealth();
        OvershieldCooldown();
    }

    private void RegenHealth()
    {
        healthSlider.value = currentHealth / maxHealth;

        if (currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }

    private void OvershieldCooldown()
    {
        if (currentInvincibilityTime > 0)
        {
            currentInvincibilityTime -= Time.deltaTime;
        }
        else
        {
            overShield.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        ApplyMovementAndRotation();
        ThrusterVisualization();
        CheckDeath();
    }

    private void ApplyMovementAndRotation()
    {
        float currentSpeed = movementSpeed;

        // Boost logic
        if (isBoosting)
        {
            currentSpeed *= boostMultiplier;
            boostAmount -= boostUsageRate * Time.fixedDeltaTime;

        }
        else if (boostAmount < maxBoost) // Recharge boost
        {
            boostAmount += boostRechargeRate * Time.fixedDeltaTime;
        }

        // Forward and backward movement
        rb.AddForce(transform.up * vertMovement * currentSpeed);

        if(vertMovement != 0)
        {
            currentEnergy -= movementEnergyCost * Time.fixedDeltaTime;
            energySlider.value = currentEnergy / maxEnergy;
            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
            }
        }

        // Rotation
        if (horzMovement != 0)
        {
            rb.AddTorque(-horzMovement * rotationSpeed);
        }

        // Clamp boost amount within bounds
        boostAmount = Mathf.Clamp(boostAmount, 0, maxBoost);
    }

    private void ThrusterVisualization()
    {
        // Activate exhaust when moving forward
        if (exhaustActive && vertMovement > 0)
        {
            leftExhhaust.SetActive(true);
            rightExhaust.SetActive(true);
        }
        else
        {
            leftExhhaust.SetActive(false);
            rightExhaust.SetActive(false);
        }

        // Thruster activation based on rotation
        if (horzMovement > 0 && exhaustActive && vertMovement == 0) // Turning Right
        {
            leftExhhaust.SetActive(true);
            rightExhaust.SetActive(false);
        }
        else if (horzMovement < 0 && exhaustActive && vertMovement == 0) // Turning Left
        {
            rightExhaust.SetActive(true);
            leftExhhaust.SetActive(false);
        }
        else if (horzMovement == 0 && vertMovement == 0) // If stationary, no thrusters
        {
            leftExhhaust.SetActive(false);
            rightExhaust.SetActive(false);
        }
    }

    private void DisableBlasters()
    {
        standardBlaster.SetActive(false);
        fireBlaster.SetActive(false);
        iceBlaster.SetActive(false);
        electricBlaster.SetActive(false);
    }

    public void SwapBlaster(int blasterType)
    {
        DisableBlasters();
        switch (blasterType)
        {
            case 0:
                standardBlaster.SetActive(true);
                break;
            case 1:
                fireBlaster.SetActive(true);
                break;
            case 2:
                iceBlaster.SetActive(true);
                break;
            case 3:
                electricBlaster.SetActive(true);
                break;
            default:
                standardBlaster.SetActive(true);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HealthPack"))
        {
            currentHealth += maxHealth * 0.2f;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            Destroy(collision.gameObject);
        }
        else if(collision.CompareTag("Crystal"))
        {
            Money.Instance.AddMoney(1);
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if(drawPlayerInteractionRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minInteractionRadius);
        }
    }

    private void CheckDeath()
    {
        if (currentHealth <= 0 || currentEnergy <= 0)
        {
            gameObject.SetActive(false);
            onDeath?.Invoke();
        }
    }
}
