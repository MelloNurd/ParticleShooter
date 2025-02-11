using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed;
    public float movementSpeed;
    public float rotationDeceleration;
    private float vertMovement;
    private float horzMovement;
    private Rigidbody2D rb;
    public bool exhaustActive;

    private ParticleSystem leftExhaust;
    private ParticleSystem rightExhaust;
    private ParticleSystem.EmissionModule leftEmission;
    private ParticleSystem.EmissionModule rightEmission;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Get the single Particle System for each thruster
        leftExhaust = transform.Find("LeftThruster").GetComponentInChildren<ParticleSystem>();
        rightExhaust = transform.Find("RightThruster").GetComponentInChildren<ParticleSystem>();

        // Get the emission modules for easy control
        leftEmission = leftExhaust.emission;
        rightEmission = rightExhaust.emission;
    }

    void Update()
    {
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Forward and backward movement
        if(vertMovement > 0)
        {
            rb.AddForce(transform.up * vertMovement * movementSpeed);
        }

        // Activate exhaust when moving forward
        if (exhaustActive && vertMovement > 0)
        {
            leftEmission.rateOverTime = 20;
            rightEmission.rateOverTime = 20;
        }
        else
        {
            // Deactivate exhaust when not moving
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
    }
}
