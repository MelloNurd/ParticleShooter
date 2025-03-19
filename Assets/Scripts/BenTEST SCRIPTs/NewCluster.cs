using NaughtyAttributes;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public enum ParticleTypes
{
    Defense,
    Speed,
    Attack,
    Projectile
}

public class NewCluster : MonoBehaviour
{
    public static NewCluster Instance;

    [SerializeField] private GameObject _particlePrefab;

    [BoxGroup("Cluster Settings")] public float PlayerControlSpeed = 5f; // Speed at which the target moves based on player input
    [BoxGroup("Cluster Settings")] public int NumberOfTypes = 4;
    [BoxGroup("Cluster Settings")] public int StartPopulation = 200;
    [BoxGroup("Cluster Settings")] public SerializedDictionary<ParticleTypes, int> ParticleTypeCounts = new SerializedDictionary<ParticleTypes, int>();
    
    [HideInInspector] public List<NewParticle> Particles = new List<NewParticle>(); // List to hold all particles in the cluster
    [ShowNativeProperty] public int ParticleCount => Particles.Count;

    [BoxGroup("Particle Physics")] public float ParticleRadius = 0.3f; // Radius for particle interactions
    [BoxGroup("Particle Physics")] public float Friction = 0.9f;
    [BoxGroup("Particle Physics")] public float ParticleSpeed = 3f;
    [BoxGroup("Particle Physics")] public float ParticleMaxSpeed = 5f; // Maximum speed for particles
    [BoxGroup("Particle Physics")] public float ParticleSteeringStrength = 1f; // Maximum speed for particles
    [BoxGroup("Particle Physics")] public Array2D<float> AttractionMatrix;

    private GameObject particleParent; // Parent object to hold all particles, NOT this gameObject

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

        // Initialize the attraction matrix with some random values
        AttractionMatrix = new Array2D<float>(NumberOfTypes, NumberOfTypes);
        for (int i = 0; i < NumberOfTypes; i++)
        {
            for (int j = 0; j < NumberOfTypes; j++)
            {
                AttractionMatrix[i, j] = Random.Range(0.01f, 1f);
            }

            // Also using this loop to initialize the particle type counts
            ParticleTypeCounts.Add(NewParticle.ParticleTypeToEnum(i), 0); // Initialize count for each type
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particleParent = new GameObject("Player Particles Holder");

        for (int i = 0; i < StartPopulation; i++)
        {
            // Instantiate a new particle and set its type
            GameObject newParticle = Instantiate(_particlePrefab, Random.insideUnitCircle, Quaternion.identity, particleParent.transform);
            ParticleTypes randomType = NewParticle.ParticleTypeToEnum(Random.Range(0, NumberOfTypes));
            newParticle.name = $"Particle {i} ({randomType.ToString()})";
            NewParticle particleComponent = newParticle.GetComponent<NewParticle>();
            if (particleComponent != null)
            {
                particleComponent.SetType(randomType); // Assuming NewParticle has a Type property
                particleComponent.Id = i;

                particleComponent.Cluster = this;
            }
        }
    }

    void Update()
    {
        // Example: using keyboard input to control the cluster's target position
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(moveX, moveY, 0);

        int speedParticleCount = ParticleTypeCounts[ParticleTypes.Speed];
        float speedBonusPerParticle = 0.1f; // 10% bonus per speed particle, adjust as needed

        // Compute a multiplier so that more type 2 particles boost the speed.
        float speedMultiplier = 1.0f + (speedParticleCount * speedBonusPerParticle);

        // Update the cluster target smoothly based on input
        transform.position += input * Time.deltaTime * PlayerControlSpeed * speedMultiplier;

        if(Input.GetKeyDown(KeyCode.P))
        {
            PrintTypes();
        }
    }

    public void ResetParticleCounts()
    {
        Particles.Clear();
        ParticleTypeCounts.Clear(); // Clear previous counts
        for(int i = 0; i < particleParent.transform.childCount; i++)
        {
            NewParticle temp = particleParent.transform.GetChild(i).GetComponent<NewParticle>();
            if (temp != null)
            {
                ParticleTypeCounts[temp.Type]++; // Increment the count for this type
                Particles.Add(temp); // Add the particle to the cluster's list
            }
        }
    }

    private void PrintTypes()
    {
        foreach (var kvp in ParticleTypeCounts)
        {
            Debug.Log($"Type {kvp.Key}: {kvp.Value} particles");
        }
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
