using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class RPGPlayer : MonoBehaviour
{
    public float movementSpeed;

    private Vector2 movement;
    private bool _canInput;

    private Rigidbody2D rb;

    private float xBoundary;
    private float yBoundary;

    private bool _isBoosting;
    private bool _canBoost;
    [SerializeField] private float _boostTime = 0.15f; // The length of the boost, i.e. how long the player will move at the boosted speed
    [SerializeField] private float _boostSpeed = 35f; // The speed that the player will move at while boosting
    [SerializeField] private float _boostCooldown = 3f; // The time it takes for the boost to be available again

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        xBoundary = ParticleManager.Instance.ScreenSpace.x / 2;
        yBoundary = ParticleManager.Instance.ScreenSpace.y / 2;

        _canInput = true;
        _canBoost = true;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        WorldSpaceWrapping();

        if (_canInput)
        {
            rb.linearVelocity = movement.normalized * movementSpeed;
        }
        if (_canBoost && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)) && movement != Vector2.zero && !_isBoosting)
        {
            StartCoroutine(Boost());
        }
    }

    private IEnumerator Boost()
    {
        _canBoost = false;
        _isBoosting = true;
        _canInput = false;
        rb.linearVelocity = movement.normalized * _boostSpeed;
        yield return new WaitForSeconds(_boostTime);
        _canInput = true;
        _isBoosting = false;
        yield return new WaitForSeconds(_boostCooldown - _boostTime);
        _canBoost = true;
    }

    private void WorldSpaceWrapping()
    {
        Vector3 newPosition = transform.position;

        if (transform.position.x > xBoundary)
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
}
