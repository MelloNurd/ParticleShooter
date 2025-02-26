using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float rotationSpeed;
    public float movementSpeed;
    public float boostMultiplier = 2f;  // Boost increases speed
    public float maxBoost = 100f;
    public float boostAmount;
    public float boostUsageRate = 20f;
    public float boostRechargeRate = 10f;

    private float vertMovement;
    private float horzMovement;
    private bool isBoosting;
    private Rigidbody2D rb;
    public bool exhaustActive;
    public int blasterType = 0;

    private ParticleSystem leftExhaust;
    private ParticleSystem rightExhaust;
    private ParticleSystem.EmissionModule leftEmission;
    private ParticleSystem.EmissionModule rightEmission;
    private GameObject standardBlaster;
    private GameObject fireBlaster;
    private GameObject iceBlaster;
    private GameObject electricBlaster;

    private Slider boostSlider;

    public Transform frontPoint;
    public Transform backPoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boostAmount = maxBoost;

        // Get the single Particle System for each thruster
        leftExhaust = transform.Find("LeftThruster").GetComponentInChildren<ParticleSystem>();
        rightExhaust = transform.Find("RightThruster").GetComponentInChildren<ParticleSystem>();

        // Get the blaster objects
        standardBlaster = transform.Find("StandardBlaster").gameObject;
        fireBlaster = transform.Find("FireBlaster").gameObject;
        iceBlaster = transform.Find("IceBlaster").gameObject;
        electricBlaster = transform.Find("ElectricBlaster").gameObject;

        boostSlider = GameObject.Find("PlayerBoost").GetComponent<Slider>();

        // Get the emission modules for easy control
        leftEmission = leftExhaust.emission;
        rightEmission = rightExhaust.emission;

        frontPoint = transform.Find("FrontAttractor");
        backPoint = transform.Find("BackAttractor");
    }

    void Update()
    {
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");

        // Check if the player is boosting
        isBoosting = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.B)) && boostAmount > 0;
        boostSlider.value = boostAmount / maxBoost;

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
            leftEmission.rateOverTime = isBoosting ? 40 : 20; // More exhaust when boosting
            rightEmission.rateOverTime = isBoosting ? 40 : 20;
        }
        else
        {
            leftEmission.rateOverTime = 0;
            rightEmission.rateOverTime = 0;
        }

        // Rotation
        if (horzMovement != 0)
        {
            rb.AddTorque(-horzMovement * rotationSpeed);
        }

        // Thruster activation based on rotation
        if (horzMovement > 0 && exhaustActive && vertMovement == 0) // Turning Right
        {
            leftEmission.rateOverTime = 10;
            rightEmission.rateOverTime = 0;
        }
        else if (horzMovement < 0 && exhaustActive && vertMovement == 0) // Turning Left
        {
            rightEmission.rateOverTime = 10;
            leftEmission.rateOverTime = 0;
        }
        else if (horzMovement == 0 && vertMovement == 0) // If stationary, no thrusters
        {
            leftEmission.rateOverTime = 0;
            rightEmission.rateOverTime = 0;
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
