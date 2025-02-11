using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static List<Particle> Particles { get; private set; } = new List<Particle>();

    private Quadtree quadtree;
    private Rect simulationBounds = new Rect(-10, -10, 20, 20); // Adjust based on simulation size

    public static float[,] Forces;
    public static float[,] MinDistances;
    public static float[,] Radii;

    private int numTypes = 3;

    public static ParticleManager Instance { get; private set; }
    public static bool IsFinishedSpawning { get; set; } = false;

    [SerializeField] private GameObject _particlePrefab;
    [SerializeField] private int _numberOfParticles = 100;

    private float _range = 10f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Make sure the particle prefab is assigned
        if (_particlePrefab == null)
        {
            Debug.LogError("Particle prefab is not assigned in the inspector.");
            return;
        }
    }

    private void Start()
    {
        Initialize();
        SpawnParticles();
    }

    private int frameCounter = 0;
    private const int UPDATE_INTERVAL = 5; // Update every 5 frames

    private void Update()
    {
        if (frameCounter++ % UPDATE_INTERVAL == 0)
        {
            quadtree = new Quadtree(simulationBounds);
            foreach (var p in Particles)
            {
                quadtree.Insert(p);
            }
        }
    }

    public List<Particle> GetNearbyParticles(Particle particle, float searchRadius)
    {
        return quadtree.QueryRange(new Rect(
            particle.transform.position.x - searchRadius,
            particle.transform.position.y - searchRadius,
            searchRadius * 2, searchRadius * 2));
    }

    private void Initialize()
    {
        Forces = new float[numTypes, numTypes];
        MinDistances = new float[numTypes, numTypes];
        Radii = new float[numTypes, numTypes];
        for (int i = 0; i < numTypes; i++)
        {
            for (int j = 0; i < numTypes; i++)
            {
                // Forces
                Forces[i, j] = Random.Range(0.3f, 1f);
                if(Random.Range(0, 1) > 0.5f)
                {
                    Forces[i, j] *= -1;
                }

                // Minimum distances
                MinDistances[i, j] = Random.Range(30, 51);
                Radii[i, j] = Random.Range(70, 251);
            }
        }
    }

    private void SpawnParticles()
    {
        for (int i = 0; i < _numberOfParticles; i++)
        {
            // Get a random position around the center 0,0
            Vector3 randomPos = new Vector3(Random.Range(-_range, _range), Random.Range(-_range, _range), 0);
            GameObject particle = Instantiate(_particlePrefab, randomPos, Quaternion.identity);
            Particle particleScript = particle.GetComponent<Particle>();
            if (particleScript != null)
            {
                ParticleManager.Particles.Add(particle.GetComponent<Particle>());
                particleScript.Type = Random.Range(0, numTypes);
                Color color = Color.HSVToRGB((float)particleScript.Type / numTypes, 1, 1);
                particle.GetComponent<SpriteRenderer>().color = color;
            }
        }
        IsFinishedSpawning = true;
    }
}
