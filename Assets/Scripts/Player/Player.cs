using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float rotationSpeed = 0.75f;
    public float movementSpeed = 5f;
    public float boostMultiplier = 1.75f;  // Boost increases speed
    public float maxBoost = 100f; // Total amount of boost the player has
    public float boostUsageRate = 20f; // How fast the boost depletes
    public float boostRechargeRate = 2; // How fast the boost recharges
    private float boostAmount; // Handles the current amount of boost the player has (how filled the bar is)

    private float vertMovement;
    private float horzMovement;
    private bool isBoosting;
    private Rigidbody2D rb;
    public bool exhaustActive;
    public int blasterType = 0;

    public float currentHealth = 100;
    public float maxHealth = 100;

    private GameObject standardBlaster;
    private GameObject fireBlaster;
    private GameObject iceBlaster;
    private GameObject electricBlaster;

    private Slider boostSlider;
    private Slider healthSlider;

    public Transform frontPoint;
    public Transform backPoint;

    private float xBoundary;
    private float yBoundary;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boostAmount = maxBoost;

        // Get the blaster objects
        standardBlaster = transform.Find("StandardBlaster").gameObject;
        fireBlaster = transform.Find("FireBlaster").gameObject;
        iceBlaster = transform.Find("IceBlaster").gameObject;
        electricBlaster = transform.Find("ElectricBlaster").gameObject;

        boostSlider = GameObject.Find("PlayerBoost").GetComponent<Slider>();
        healthSlider = GameObject.Find("PlayerHealth").GetComponent<Slider>();

        frontPoint = transform.Find("FrontAttractor");
        backPoint = transform.Find("BackAttractor");

        xBoundary = ParticleManager.Instance.ScreenSpace.x / 2;
        yBoundary = ParticleManager.Instance.ScreenSpace.y / 2;
    }

    void Update()
    {
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");

        // Check if the player is boosting
        isBoosting = Input.GetKey(KeyCode.LeftShift) && boostAmount > 0 && vertMovement != 0;
        boostSlider.value = boostAmount / maxBoost;

        healthSlider.value = currentHealth / maxHealth;

        // Screen wrapping
        Vector3 newPosition = transform.position;

        if(transform.position.x > xBoundary)
        {
            newPosition.x = -xBoundary;
        }
        else if (transform.position.x < -xBoundary)
        {
            newPosition.x = xBoundary;
        }   
        if (transform.position.y > yBoundary)
        {
            newPosition.y = -yBoundary;
        }
        else if (transform.position.y < -yBoundary)
        {
            newPosition.y = yBoundary;
        }
        transform.position = newPosition;

    }

    private void FixedUpdate()
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
        if (vertMovement > 0)
        {
            rb.AddForce(transform.up * vertMovement * currentSpeed);
        }

        // Activate exhaust when moving forward
        if (exhaustActive && vertMovement > 0)
        {
            //leftEmission.rateOverTime = isBoosting ? 40 : 20; // More exhaust when boosting
            //rightEmission.rateOverTime = isBoosting ? 40 : 20;
        }
        else
        {
            //leftEmission.rateOverTime = 0;
            //rightEmission.rateOverTime = 0;
        }

        // Rotation
        if (horzMovement != 0)
        {
            rb.AddTorque(-horzMovement * rotationSpeed);
        }

        // Thruster activation based on rotation
        if (horzMovement > 0 && exhaustActive && vertMovement == 0) // Turning Right
        {
            //leftEmission.rateOverTime = 10;
            //rightEmission.rateOverTime = 0;
        }
        else if (horzMovement < 0 && exhaustActive && vertMovement == 0) // Turning Left
        {
            //rightEmission.rateOverTime = 10;
            //leftEmission.rateOverTime = 0;
        }
        else if (horzMovement == 0 && vertMovement == 0) // If stationary, no thrusters
        {
            //leftEmission.rateOverTime = 0;
            //rightEmission.rateOverTime = 0;
        }

        // Clamp boost amount within bounds
        boostAmount = Mathf.Clamp(boostAmount, 0, maxBoost);
    }

    private void disableBlasters()
    {
        standardBlaster.SetActive(false);
        fireBlaster.SetActive(false);
        iceBlaster.SetActive(false);
        electricBlaster.SetActive(false);
    }

    public void InitializeSpace()
    {
        xBoundary = ParticleManager.Instance.ScreenSpace.x / 2;
        yBoundary = ParticleManager.Instance.ScreenSpace.y / 2;
    }

    public void swapBlaster(int blasterType)
    {
        disableBlasters();
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
}
