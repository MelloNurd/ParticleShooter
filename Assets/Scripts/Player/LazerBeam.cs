using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LazerBeam : MonoBehaviour
{

    public bool IsAttacking = false;

    [SerializeField] private float _fireRange;
    [SerializeField] private int _maxHits; // How many particles the beam can hit, aka pierce

    private int _particleLayer;

    private LineRenderer _lr;

    private void Start()
    {
        _particleLayer = 1 << LayerMask.NameToLayer("Particle"); // We actually get the inverted (1 << ...) layer mask, so we can use it in the Physics2D.BoxCastAll method

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        // Doing this since we have the RequireComponent, and the component may/will be added at runtime, meaning all values need to be set
        _lr = GetComponent<LineRenderer>();
        _lr.material = new Material(Shader.Find("Sprites/Default"));
        _lr.widthCurve = new AnimationCurve(new Keyframe(0, 0.1f), new Keyframe(1, 0.5f));
        _lr.colorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            },
            alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.4f, 0),
                new GradientAlphaKey(0.7f, 1)
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        IsAttacking = Input.GetMouseButton(0);
        if(IsAttacking)
        {
            FireBeam();
        }
        else
        {
            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, transform.position);
        }
    }

    private void FireBeam()
    {
        // Cast a beam from the player. This returns all objects hit, in order from distance.
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.5f, 0.5f), 0, transform.up, _fireRange, _particleLayer);

        _lr.SetPosition(0, transform.position);

        // If there are no hits, we use the fireRange from the player. If there are hits, we use the distance to the first hit.
        Vector3 hitPoint = hits.Length > 0 ? hits[0].point : transform.position + transform.up * _fireRange;
        _lr.SetPosition(1, hitPoint);

        for (int i = 0; i < hits.Length; i++)
        {
            if(i >= _maxHits) break; // Only hit the first _maxHits particles, instead of just going through all

            if (hits[i].transform.TryGetComponent(out ParticleHealth particle)) {
                particle.Damage(2);
            }
        }
    }
}
