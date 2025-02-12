using UnityEngine;

public class ParticleLife : MonoBehaviour
{
    struct Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public int type;
    }

    public ComputeShader computeShader;
    public Material particleMaterial;
    public int particleCount = 1000;
    public Vector2 bounds = new Vector2(10, 10);

    private ComputeBuffer particleBuffer;
    private Particle[] particles;
    private int kernel;

    void Start()
    {
        kernel = computeShader.FindKernel("CSMain");
        particles = new Particle[particleCount];

        // Initialize particles
        for (int i = 0; i < particleCount; i++)
        {
            particles[i].position = new Vector2(Random.value * bounds.x, Random.value * bounds.y);
            particles[i].velocity = Vector2.zero;
            particles[i].type = Random.Range(0, 3);
        }

        // Create Compute Buffer
        particleBuffer = new ComputeBuffer(particleCount, sizeof(float) * 4 + sizeof(int));
        particleBuffer.SetData(particles);

        // Set buffer and other variables in Compute Shader
        computeShader.SetInt("particleCount", particleCount);
        computeShader.SetVector("bounds", bounds);
        computeShader.SetBuffer(kernel, "particles", particleBuffer); // ✅ Ensure this line is here

        // Interaction matrix (adjust values for attraction/repulsion)
        float[] interactionMatrix = {
            0.1f, -0.2f, 0.05f,
            -0.2f, 0.1f, 0.3f,
            0.05f, 0.3f, -0.1f
        };
        computeShader.SetFloats("interactionMatrix", interactionMatrix);
    }

    void Update()
    {
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetBuffer(kernel, "particles", particleBuffer); // ✅ Ensure this line is here
        computeShader.Dispatch(kernel, Mathf.CeilToInt(particleCount / 256f), 1, 1);
    }


    void OnRenderObject()
    {
        particleMaterial.SetPass(0);
        particleMaterial.SetBuffer("particles", particleBuffer);

        Graphics.DrawProceduralNow(MeshTopology.Points, particleCount, 1);
    }

    void OnDestroy()
    {
        particleBuffer.Release();
    }
}
