using UnityEngine;

public class NewParticle : MonoBehaviour
{
    public int Type;
    public int Id;

    private Rigidbody2D _rb;

    private Vector2 force;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Compute forces from nearby particles as before
        force = Vector2.zero;
        Vector2 interactionForce = Vector2.zero;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out NewParticle otherParticle))
            {
                float distance = Vector2.Distance(transform.position, otherParticle.transform.position);
                Vector2 direction = (otherParticle.transform.position - transform.position).normalized;
                interactionForce += direction * CalculateForce(distance, NewCluster.Instance.AttractionMatrix[Type, otherParticle.Type]);
            }
        }

        // 2. Compute the arrival steering force based on distance to the target.
        Vector2 targetPos = (Vector2)NewCluster.Instance.transform.position;
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float distanceToTarget = toTarget.magnitude;

        float desiredSpeed = Mathf.Min(distanceToTarget * NewCluster.Instance.ParticleSpeed, NewCluster.Instance.ParticleMaxSpeed);
        Vector2 desiredVelocity = toTarget.normalized * desiredSpeed;

        Vector2 steeringForce = (desiredVelocity - _rb.linearVelocity) * NewCluster.Instance.ParticleSteeringStrength;

        // 3. Combine the forces. Use a lower weight for the steering force.
        float arrivalWeight = 0.2f; // Lower weight to preserve natural cell behavior
        force = interactionForce + (steeringForce * arrivalWeight);
    }

    private void FixedUpdate()
    {
        _rb.AddForce(force, ForceMode2D.Force);
        _rb.linearVelocity *= NewCluster.Instance.Friction; // Apply friction to slow down the particles
    }

    private float CalculateForce(float distance, float attractionValue)
    {
        float radius = NewCluster.Instance.ParticleRadius;

        if (distance < radius)
        {
            return (distance / radius) - 1;
        }
        else if (radius < distance && distance < 1)
        {
            return attractionValue * (1 - Mathf.Abs(2 * distance - 1 - radius) / (1 - radius));
        }

        return 0;
    }

    public void SetType(int newType)
    {
        // Set type and also color based on type
        Color color = NewCluster.GetColorByType(newType);
        GetComponent<SpriteRenderer>().color = color;

        Type = newType;
    }
}
