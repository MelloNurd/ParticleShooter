using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed;
    public float movementSpeed;
    public float rotationDeceleration;
    private float vertMovement;
    private float horzMovement;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get the input from the player
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Move the player forward or backward
        rb.AddForce(transform.up * vertMovement * movementSpeed);

        // Rotate the player with deceleration on the rotation to add a drift effect
        if (horzMovement != 0)
        {
            rb.AddTorque(-horzMovement * rotationSpeed);
        }
        else
        {
            rb.angularVelocity *= rotationDeceleration;
        }
    }
}
