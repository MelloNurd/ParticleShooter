using UnityEngine;

public class Zones : MonoBehaviour
{
    public GameObject circlePrefab;

    public string homebaseName = "Homebase";

    public int circleCount = 5;

    public float scaleIncrement = 0.2f;

    public float exponentialFactor = 0.25f;

    public int sortingOrder = -15;

    private Color startColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    private Color endColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    void Start()
    {
        // Locate the Homebase object in the scene
        GameObject homebase = GameObject.Find(homebaseName);
        if (homebase == null)
        {
            Debug.LogError("Homebase object not found. Please ensure an object with the name " + homebaseName + " exists in the scene.");
            return;
        }

        // Use Homebase position as the origin
        Vector3 origin = homebase.transform.position;

        // Create circles incrementally scaling them larger
        for (int i = 0; i < circleCount; i++)
        {
            // Instantiate a new circle at the home base position
            GameObject circle = Instantiate(circlePrefab, origin, Quaternion.identity);

            // Calculate scale multiplier so each circle is larger than the previous one
            float scaleMultiplier = 1 + ((i+1) * scaleIncrement * 1);
            scaleMultiplier *= scaleMultiplier * exponentialFactor;
            circle.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1);

            // If the circle prefab has a SpriteRenderer, set its sorting order and interpolate its color.
            SpriteRenderer sr = circle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = sortingOrder - i;
                // Calculate lerp factor t, ensuring we don't run into division by zero if circleCount is 1.
                float t = circleCount > 1 ? (float)i / (circleCount - 1) : 0;
                sr.color = Color.Lerp(startColor, endColor, t);
            }
        }
    }

    void Update()
    {
        // Update logic if needed
    }
}
