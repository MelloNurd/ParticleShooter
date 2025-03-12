using UnityEngine;

public class RoguelikePlayer : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool canInput;

    public float movementSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        canInput = true;
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (canInput)
        {
            rb.linearVelocity = movement.normalized * movementSpeed;
        }
    }
}
