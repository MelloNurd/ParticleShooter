using UnityEngine;

public class ParticleComputeManager : MonoBehaviour
{
    struct Particle
    {
        public Vector3 position;  // 12 bytes
        public Vector3 velocity;  // 12 bytes
        public int type;          // 4 bytes
        public float padding;     // 4 bytes (Fixes 28 → 32 bytes issue)
    }


    public ComputeShader computeShader;
    public int numParticles = 2000;
    public int numTypes = 3;
    public GameObject particlePrefab;

    private ComputeBuffer particleBuffer, forcesBuffer, minDistancesBuffer;
    private Particle[] particles;
    private float[] forces, minDistances;
    private GameObject[] particleObjects;

    void Start()
    {
        InitParticles();
    }

    void InitParticles()
    {
        particles = new Particle[numParticles];
        particleObjects = new GameObject[numParticles];
        forces = new float[numTypes * numTypes];
        minDistances = new float[numTypes * numTypes];

        // Initialize force and distance matrices
        for (int i = 0; i < numTypes; i++)
        {
            for (int j = 0; j < numTypes; j++)
            {
                int index = i * numTypes + j;
                forces[index] = Random.Range(-1f, 1f);
                minDistances[index] = Random.Range(30f, 50f);
            }
        }

        // Initialize particles
        for (int i = 0; i < numParticles; i++)
        {
            particles[i] = new Particle
            {
                position = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0),
                velocity = Vector3.zero,
                type = Random.Range(0, numTypes)
            };

            particleObjects[i] = Instantiate(particlePrefab, particles[i].position, Quaternion.identity);
            particleObjects[i].GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)particles[i].type / numTypes, 1, 1);
        }

        // Create compute buffers
        particleBuffer = new ComputeBuffer(numParticles, sizeof(float) * 7 + sizeof(int));
        forcesBuffer = new ComputeBuffer(numTypes * numTypes, sizeof(float));
        minDistancesBuffer = new ComputeBuffer(numTypes * numTypes, sizeof(float));

        particleBuffer.SetData(particles);
        forcesBuffer.SetData(forces);
        minDistancesBuffer.SetData(minDistances);

        computeShader.SetBuffer(0, "particles", particleBuffer);
        computeShader.SetBuffer(0, "forces", forcesBuffer);
        computeShader.SetBuffer(0, "minDistances", minDistancesBuffer);
        computeShader.SetInt("numParticles", numParticles);
        computeShader.SetInt("numTypes", numTypes);
    }

    void Update()
    {
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("dampening", 0.05f);
        computeShader.SetFloat("friction", 0.85f);

        computeShader.Dispatch(0, numParticles / 256 + 1, 1, 1);

        // Read back positions and update visuals
        particleBuffer.GetData(particles);
        for (int i = 0; i < numParticles; i++)
        {
            particleObjects[i].transform.position = particles[i].position;
        }
    }

    void OnDestroy()
    {
        if (particleBuffer != null) particleBuffer.Release();
        if (forcesBuffer != null) forcesBuffer.Release();
        if (minDistancesBuffer != null) minDistancesBuffer.Release();
    }
}
