using System.Collections.Generic;
using UnityEngine;

public class Zones : MonoBehaviour
{
    public static Zones Instance { get; private set; }

    public GameObject circlePrefab;

    public string homebaseName = "Homebase";

    public int circleCount = 5;
    public int NumberOfZones => circleCount + 1; // We count outside the final circle as a zone too
    public float scaleIncrement = 0.2f;
    public float exponentialFactor = 0.25f;
    public int sortingOrder = -15;

    // Private colors remain unchanged
    private Color startColor = new Color(0.05f, 0.05f, 0.15f, 1f);
    private Color endColor = new Color(0f, 0f, 0.05f, 1f);

    // Cache for runtime regeneration
    private List<GameObject> rings = new List<GameObject>();
    public List<float> ringRadiuses = new List<float>();
    private int prevCircleCount;
    private float prevScaleIncrement;
    private float prevExponentialFactor;
    private int prevSortingOrder;

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

        Camera.main.backgroundColor = Color.black;
    }

    void Start()
    {
        CacheParameters();
        RegenerateRings();
    }

    void Update()
    {
        // Check if any of the inspector parameters have changed.
        if (circleCount != prevCircleCount ||
            scaleIncrement != prevScaleIncrement ||
            exponentialFactor != prevExponentialFactor ||
            sortingOrder != prevSortingOrder)
        {
            RegenerateRings();
            CacheParameters();
        }
    }

    void CacheParameters()
    {
        prevCircleCount = circleCount;
        prevScaleIncrement = scaleIncrement;
        prevExponentialFactor = exponentialFactor;
        prevSortingOrder = sortingOrder;
    }

    void RegenerateRings()
    {
        // Destroy previous rings
        foreach (var ring in rings)
        {
            if (ring != null)
            {
                Destroy(ring);
            }
        }
        rings.Clear();
        ringRadiuses.Clear();

        // Locate the Homebase object
        GameObject homebase = GameObject.Find(homebaseName);
        if (homebase == null)
        {
            Debug.LogError("Homebase object not found. Please ensure an object with the name " + homebaseName + " exists in the scene.");
            return;
        }

        // Use Homebase position as the origin
        Vector3 origin = homebase.transform.position;

        // Create the rings with incremental scale calculations.
        for (int i = 0; i < circleCount; i++)
        {
            // Instantiate a new circle at the homebase's position
            GameObject circle = Instantiate(circlePrefab, origin, Quaternion.identity);

            // Calculate the scale multiplier so that each ring is larger than the last.
            float scaleMultiplier = 1 + ((i + 1) * scaleIncrement);
            scaleMultiplier *= scaleMultiplier * exponentialFactor;
            circle.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1);

            // Adjust the SpriteRenderer if it exists.
            SpriteRenderer sr = circle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = sortingOrder - i;
                // Compute the interpolation between the start and end colors.
                float t = circleCount > 1 ? (float)i / (circleCount - 1) : 0;
                sr.color = Color.Lerp(startColor, endColor, t);
            }

            // Keep track of the instantiated ring.
            rings.Add(circle);
            ringRadiuses.Add(circle.transform.localScale.x / 2);
        }
    }

    public static int GetCurrentZone(Vector3 position)
    {
        if (Instance == null) return -1;

        for (int i = 0; i < Instance.rings.Count; i++)
        {
            if (Instance.rings[i] != null)
            {
                float distance = Vector3.Distance(position, Instance.rings[i].transform.position);
                if (distance <= Instance.rings[i].transform.localScale.x / 2)
                {
                    return i;
                }
            }
        }
        return -1; // Not in any zone
    }

    public List<GameObject> GetZones()
    {
        return rings;
    }
}
