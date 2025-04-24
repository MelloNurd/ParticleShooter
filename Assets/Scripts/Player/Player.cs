using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [BoxGroup("Game Settings")] public float minInteractionRadius = 20f;
    [BoxGroup("Game Settings")][Tooltip("This only is visible in Scene view.")][SerializeField]
    private bool drawPlayerInteractionRadius = false;

    [BoxGroup("Movement Settings")] public float rotationSpeed = 0.75f;
    [BoxGroup("Movement Settings")] public float movementSpeed = 5f;
    [BoxGroup("Movement Settings")] public float boostMultiplier = 1.75f;  // Boost increases speed
    [BoxGroup("Movement Settings")] public float maxBoost = 100f; // Total amount of boost the player has
    [BoxGroup("Movement Settings")] public float boostUsageRate = 20f; // How fast the boost depletes
    [BoxGroup("Movement Settings")] public float boostRechargeRate = 2; // How fast the boost recharges
    private float boostAmount; // Handles the current amount of boost the player has (how filled the bar is)

    [ShowNativeProperty] public int CurrentZone => Zones.GetCurrentZone(transform.position);

    private float vertInput;
    private float horzInput;
    private bool hasInput => horzInput != 0 || vertInput != 0;

    private bool isBoosting;
    [BoxGroup("Thruster Settings")] public bool exhaustActive;
    private ThrusterVisualizer leftThruster;
    private ThrusterVisualizer rightThruster;
    public bool movingForwards => vertInput > 0;
    public bool movingBackwards => vertInput < 0;
    private bool CanMoveBackwards = false;

    [BoxGroup("Health Settings")] public float currentHealth = 100;
    [BoxGroup("Health Settings")] public float maxHealth = 100;
    [BoxGroup("Health Settings")] public float healthRegenRate = 1f;

    private GameObject standardBlaster;
    private GameObject fireBlaster;
    private GameObject iceBlaster;
    private GameObject electricBlaster;

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

    private bool isRecharging = false;

    [BoxGroup("Autonomous Mode")] public bool enableAutoMode = false;
    [BoxGroup("Autonomous Mode")][SerializeField] private Vector3 destinationPoint;
    private float timeStuck = 0f;

    private bool canDash = false;
    private float dashCooldown = 10;
    private float currentDashCooldown = 0;
    private float dashCost = 50f;

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
        InitializeFromUpgrades();

        rb = GetComponent<Rigidbody2D>();
        boostAmount = maxBoost;

        // Get the blaster objects
        standardBlaster = transform.Find("Weapon").gameObject;

        boostSlider = GameObject.Find("PlayerBoost").GetComponent<Slider>();
        healthSlider = GameObject.Find("PlayerHealth").GetComponent<Slider>();
        energySlider = GameObject.Find("PlayerEnergy").GetComponent<Slider>();

        leftThruster = transform.Find("LeftThruster").GetComponentInChildren<ThrusterVisualizer>();
        rightThruster = transform.Find("RightThruster").GetComponentInChildren<ThrusterVisualizer>();

        overShield = transform.Find("OverShield").gameObject;
        overShield.SetActive(true);

        if (!consumeEnergy)
        {
            movementEnergyCost = 0;
        }

        destinationPoint = new Vector3(0, -50);
    }

    [Button]
    private void SetRandomAutonomousDestination()
    {
        if (!Application.isPlaying) return; // Since we have an inspector button

        destinationPoint = Utilities.GetEmptyPointInCircle(transform.position, 25f);
    }

    void Update()
    {
        // Movement input
        if (enableAutoMode)
        {
            // Move towards the destination point
            vertInput = (Vector3.Dot(transform.right, (destinationPoint - transform.position).normalized) < 0.5f) ? 1f : 0f;

            horzInput = Vector3.Dot(transform.right, (destinationPoint - transform.position).normalized) > 0 ? 1f : -1f;

            if(rb.linearVelocity.magnitude < 1f)
            {
                timeStuck += Time.deltaTime;
            }

            if(timeStuck > 5f || Vector2.Distance(transform.position, destinationPoint) < 1)
            {
                SetRandomAutonomousDestination();
                timeStuck = 0;
            }
        }
        else
        {
            horzInput = Input.GetAxisRaw("Horizontal");
            vertInput = Input.GetAxisRaw("Vertical");
            if(!CanMoveBackwards && vertInput < 0)
            {
                vertInput = 0;
            }
        }

        // Boosting input
        isBoosting = Input.GetKey(KeyCode.LeftShift) && boostAmount > 0 && vertInput != 0;
        boostSlider.value = boostAmount / maxBoost;

        RegenHealth();
        OvershieldCooldown();

        // Energy Handling
        if (vertInput != 0 && !enableAutoMode) // Decreasing
        {
            if (!isRecharging)
            {
                currentEnergy -= movementEnergyCost * Time.fixedDeltaTime;
            }
            energySlider.value = currentEnergy / maxEnergy;
            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
            }
        }
        if (isRecharging) // Recharging
        {
            currentEnergy += maxEnergy * 0.2f * Time.deltaTime;
            energySlider.value = currentEnergy / maxEnergy;
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
            boostAmount += maxBoost * 0.2f * Time.deltaTime;
            boostSlider.value = boostAmount / maxBoost;
            if (boostAmount > maxBoost)
            {
                boostAmount = maxBoost;
            }
        }

        currentDashCooldown -= Time.deltaTime;
        if (canDash && Input.GetKeyDown(KeyCode.Space) && currentDashCooldown <= 0 && boostAmount >= dashCost)
        {
            rb.AddForce(transform.up * 10f, ForceMode2D.Impulse);
            boostAmount -= dashCost;
            currentDashCooldown = dashCooldown;
        }
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
            if(!isRecharging)
            {
                boostAmount -= boostUsageRate * Time.fixedDeltaTime;
            }
        }
        else if (boostAmount < maxBoost) // Recharge boost
        {
            boostAmount += boostRechargeRate * Time.fixedDeltaTime;
        }

        // Forward and backward movement
        rb.AddForce(transform.up * vertInput * currentSpeed);

        // Rotation
        if (horzInput != 0)
        {
            rb.AddTorque(-horzInput * rotationSpeed);
        }

        // Clamp boost amount within bounds
        boostAmount = Mathf.Clamp(boostAmount, 0, maxBoost);
    }

    private void ThrusterVisualization()
    {
        if(!hasInput || !exhaustActive)
        {
            leftThruster.SetThrust(0);
            rightThruster.SetThrust(0);
            return;
        }

        float boostMultiplier = isBoosting && boostAmount > 0 ? 1.8f : 1f;

        leftThruster.SetThrust(rb.linearVelocity.magnitude * 0.75f * boostMultiplier - (rb.angularVelocity * 0.005f));
        rightThruster.SetThrust(rb.linearVelocity.magnitude * 0.75f * boostMultiplier + (rb.angularVelocity * 0.005f));

        //if (vertInput > 0)
        //{
        //    leftExhhaust.SetActive(true);
        //    rightExhaust.SetActive(true);
        //}
        //else
        //{
        //    leftExhhaust.SetActive(false);
        //    rightExhaust.SetActive(false);
        //}

        //// Thruster activation based on rotation
        //if (horzInput > 0 && vertInput == 0) // Turning Right
        //{
        //    leftExhhaust.SetActive(true);
        //    rightExhaust.SetActive(false);
        //}
        //else if (horzInput < 0 && vertInput == 0) // Turning Left
        //{
        //    rightExhaust.SetActive(true);
        //    leftExhhaust.SetActive(false);
        //}
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
        if(collision.CompareTag("Crystal"))
        {
            Money.Instance.AddMoney(1);
            Destroy(collision.gameObject);
        }
        if(collision.CompareTag("EnergyRecharger"))
        {
           isRecharging = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("EnergyRecharger"))
        {
            isRecharging = false;
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

    private void InitializeFromUpgrades()
    {
        maxHealth = UpgradeList.HealthValues[PlayerPrefs.GetInt("Health_Upgrade", 0)];
        maxEnergy = UpgradeList.EnergyValues[PlayerPrefs.GetInt("Energy_Upgrade", 0)];
        maxBoost = UpgradeList.BoostValues[PlayerPrefs.GetInt("Boost_Upgrade", 0)];
        movementSpeed = UpgradeList.SpeedValues[PlayerPrefs.GetInt("Speed_Upgrade", 0)];

        CanMoveBackwards = PlayerPrefs.GetInt("MoveBackwards_Upgrade", 0) != 0;
        if (PlayerPrefs.GetInt("Dash_Upgrade", 0) > 0)
        {
            Debug.Log("Dash Upgrade Initialized");
            canDash = true;
            dashCooldown = UpgradeList.DashValues[PlayerPrefs.GetInt("Dash_Upgrade", 10)];
            Debug.Log("Dash Cooldown: " + dashCooldown);
        }
    }
}
