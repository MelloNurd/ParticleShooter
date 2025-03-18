using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NewCluster : MonoBehaviour
{
    public static NewCluster Instance;

    public static Vector3 ClusterTargetPos => Instance.transform.position;

    public float ParticleRadius = 0.3f; // Radius for particle interactions
    public float Friction = 0.9f;
    public float ParticleSpeed = 3f;
    public float ParticleMaxSpeed = 5f; // Maximum speed for particles
    public float ParticleSteeringStrength = 1f; // Maximum speed for particles
    public Array2D<float> AttractionMatrix;

    public float controlSpeed = 5f; // Speed at which the target moves based on player input

    public int NumberOfTypes = 4;

    public int StartPopulation = 200;

    [SerializeField] private GameObject _particlePrefab;

    private List<Dictionary<int, int>()> _particleTypeCounts = new List<Dictionary<int, int>>();

    private void Awake()
    {
        // Singleton Implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        AttractionMatrix = new Array2D<float>(NumberOfTypes, NumberOfTypes);

        // Initialize the attraction matrix with some values
        for (int i = 0; i < NumberOfTypes; i++)
        {
            for (int j = 0; j < NumberOfTypes; j++)
            {
                AttractionMatrix[i, j] = Random.Range(0.01f, 1f);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < StartPopulation; i++)
        {
            // Instantiate a new particle and set its type
            GameObject newParticle = Instantiate(_particlePrefab, Random.insideUnitCircle * 1, Quaternion.identity);
            NewParticle particleComponent = newParticle.GetComponent<NewParticle>();
            if (particleComponent != null)
            {
                int randomType = Random.Range(0, NumberOfTypes);
                particleComponent.SetType(randomType); // Assuming NewParticle has a Type property
                particleComponent.Id = i;
            }
        }
    }

    void Update()
    {
        // Example: using keyboard input to control the cluster's target position
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(moveX, moveY, 0);

        // Update the cluster target smoothly based on input
        transform.position += input * Time.deltaTime * controlSpeed;
    }

    public static Color GetColorByType(int type)
    {
        if (Instance.NumberOfTypes <= 1)
        {
            // Default to red if there's only one type
            return Color.HSVToRGB(0f, 1f, 1f);
        }

        float hue = (float)type / Instance.NumberOfTypes;

        Color color = Color.HSVToRGB(hue, 1f, 1f);

        return color;
    }
}
