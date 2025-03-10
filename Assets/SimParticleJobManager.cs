using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.Runtime.InteropServices;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;

public class SimParticleJobManager : MonoBehaviour
{
    // Interaction arrays (flattened, size: numTypes * numTypes)
    public float[] forces;
    public float[] minDistances;
    public float[] radii;

    // References to Particle GameObjects (populated by your SimParticleManager)
    private SimParticle[] particles;
    private List<int> particlesToRemove = new List<int>();

    // Native arrays for simulation data and interaction parameters.
    // Double-buffered arrays:
    private NativeArray<ParticleData> particleArrayRead;
    private NativeArray<ParticleData> particleArrayWrite;
    private NativeArray<float> nativeForces;
    private NativeArray<float> nativeMinDistances;
    private NativeArray<float> nativeRadii;

    // Job handle for scheduling.
    private JobHandle jobHandle;

    private bool _isReady;

    public void AssignTables(float[,] forcesMatrix, float[,] minDistancesMatrix, float[,] radiiMatrix)
    {
        int numTypes = SimParticleManager.Instance.NumberOfTypes;
        int numTypesSquared = numTypes * numTypes;

        // Flattening values of arrays.
        int arrayIndex = 0;
        forces = new float[numTypesSquared];
        for (int i = 0; i < numTypes; i++)
        {
            for (int j = 0; j < numTypes; j++)
            {
                forces[arrayIndex] = forcesMatrix[i, j];
                arrayIndex++;
            }
        }

        arrayIndex = 0;
        minDistances = new float[numTypesSquared];
        for (int i = 0; i < numTypes; i++)
        {
            for (int j = 0; j < numTypes; j++)
            {
                minDistances[arrayIndex] = minDistancesMatrix[i, j];
                arrayIndex++;
            }
        }

        arrayIndex = 0;
        radii = new float[numTypesSquared];
        for (int i = 0; i < numTypes; i++)
        {
            for (int j = 0; j < numTypes; j++)
            {
                radii[arrayIndex] = radiiMatrix[i, j];
                arrayIndex++;
            }
        }

        // Copy interaction arrays into native arrays.
        nativeForces = new NativeArray<float>(forces, Allocator.Persistent);
        nativeMinDistances = new NativeArray<float>(minDistances, Allocator.Persistent);
        nativeRadii = new NativeArray<float>(radii, Allocator.Persistent);
    }

    public void Initialize()
    {
        Debug.Log($"Initializing Jobs with {SimParticleManager.Particles.Count} particles.");

        // Get the particles from your SimParticleManager.
        particles = SimParticleManager.Particles.ToArray();
        int numParticles = particles.Length;

        // Allocate two NativeArrays for double buffering.
        particleArrayRead = new NativeArray<ParticleData>(numParticles, Allocator.Persistent);
        particleArrayWrite = new NativeArray<ParticleData>(numParticles, Allocator.Persistent);

        // Initialize both buffers with the current particle data.
        for (int i = 0; i < numParticles; i++)
        {
            ParticleData pd = new ParticleData
            {
                position = particles[i].transform.position,
                velocity = Vector3.zero,
                type = particles[i].Type,
            };
            particleArrayRead[i] = pd;
            particleArrayWrite[i] = pd;
        }

        _isReady = true;
    }

    private void Update()
    {
        if (!_isReady) return;

        // Remove particles if any
        if (particlesToRemove.Count > 0)
        {
            RemoveParticles();
        }

        int numParticles = particleArrayRead.Length;

        // Set up and schedule the job using the double-buffered arrays.
        SimParticleJob job = new SimParticleJob
        {
            inputParticles = particleArrayRead,
            outputParticles = particleArrayWrite,
            forces = nativeForces,
            minDistances = nativeMinDistances,
            radii = nativeRadii,
            numParticles = numParticles,
            numTypes = SimParticleManager.Instance.NumberOfTypes,
            deltaTime = Time.deltaTime,
            friction = SimParticleManager.Instance.Friction,
            dampening = SimParticleManager.Instance.Dampening,
            repulsionEffector = SimParticleManager.Instance.RepulsionEffector,
            // Pass screen parameters for world wrapping.
            screenSpace = SimParticleManager.Instance.ScreenSpace,         // For example: (ScreenSpace.x, ScreenSpace.y)
            halfScreenSpace = SimParticleManager.Instance.HalfScreenSpace      // For example: (HalfScreenSpace.x, HalfScreenSpace.y)
        };

        // Schedule with a batch size (64 particles per batch).
        jobHandle = job.Schedule(numParticles, 256);
        jobHandle.Complete();

        // Update the GameObjects with the new positions.
        for (int i = 0; i < numParticles; i++)
        {
            particles[i].transform.position = particleArrayWrite[i].position;
        }

        // Swap the buffers so the output becomes the input for the next frame.
        NativeArray<ParticleData> temp = particleArrayRead;
        particleArrayRead = particleArrayWrite;
        particleArrayWrite = temp;
    }

    private void OnDestroy()
    {
        // Dispose of all native arrays.
        if (particleArrayRead.IsCreated) particleArrayRead.Dispose();
        if (particleArrayWrite.IsCreated) particleArrayWrite.Dispose();
        if (nativeForces.IsCreated) nativeForces.Dispose();
        if (nativeMinDistances.IsCreated) nativeMinDistances.Dispose();
        if (nativeRadii.IsCreated) nativeRadii.Dispose();
    }

    public void RequestParticleRemoval(SimParticle particle)
    {
        int index = System.Array.IndexOf(particles, particle);
        if (index >= 0 && !particlesToRemove.Contains(index))
        {
            particlesToRemove.Add(index);
        }
    }

    private void RemoveParticles()
    {
        // Sort indices in descending order to avoid reindexing issues
        particlesToRemove.Sort((a, b) => b.CompareTo(a));

        foreach (int index in particlesToRemove)
        {
            // Remove from particles array
            var tempList = particles.ToList();
            tempList.RemoveAt(index);
            particles = tempList.ToArray();

            // Remove from native arrays
            RemoveAtNativeArray(ref particleArrayRead, index);
            RemoveAtNativeArray(ref particleArrayWrite, index);
        }

        particlesToRemove.Clear();
    }

    // Helper method to remove element at index from NativeArray
    private void RemoveAtNativeArray(ref NativeArray<ParticleData> array, int index)
    {
        if (index < 0 || index >= array.Length) return;

        NativeArray<ParticleData> newArray = new NativeArray<ParticleData>(array.Length - 1, Allocator.Persistent);
        if (index > 0)
            NativeArray<ParticleData>.Copy(array, 0, newArray, 0, index);

        if (index < array.Length - 1)
            NativeArray<ParticleData>.Copy(array, index + 1, newArray, index, array.Length - index - 1);

        array.Dispose();
        array = newArray;
    }
}

[BurstCompile]
public struct SimParticleJob : IJobParallelFor
{
    // Read-only input buffer.
    [Unity.Collections.ReadOnly] public NativeArray<ParticleData> inputParticles;
    // Output buffer where each thread writes its own result.
    public NativeArray<ParticleData> outputParticles;

    [Unity.Collections.ReadOnly] public NativeArray<float> forces;
    [Unity.Collections.ReadOnly] public NativeArray<float> minDistances;
    [Unity.Collections.ReadOnly] public NativeArray<float> radii;

    public int numParticles;
    public int numTypes;
    public float deltaTime;
    public float friction;
    public float dampening;
    public float repulsionEffector;

    // World-space parameters for wrapping.
    public Vector2 screenSpace;
    public Vector2 halfScreenSpace;

    public void Execute(int i)
    {
        ParticleData self = inputParticles[i];

        Vector3 totalForce = Vector3.zero;

        for (int j = 0; j < numParticles; j++)
        {
            if (i == j)
                continue;

            ParticleData other = inputParticles[j];

            // Calculate direction and apply world wrapping adjustments for distance calculation.
            Vector3 direction = other.position - self.position;
            if (direction.x > halfScreenSpace.x)
                direction.x -= screenSpace.x;
            if (direction.x < -halfScreenSpace.x)
                direction.x += screenSpace.x;
            if (direction.y > halfScreenSpace.y)
                direction.y -= screenSpace.y;
            if (direction.y < -halfScreenSpace.y)
                direction.y += screenSpace.y;

            float distance = direction.magnitude;
            if (distance > 0f)
            {
                direction.Normalize();
                int paramIndex = self.type * numTypes + other.type;
                float minDist = minDistances[paramIndex];
                float interactRadius = radii[paramIndex];
                float forceValue = forces[paramIndex];

                // Repulsive force calculation.
                if (distance < minDist)
                {
                    float scale = 1f - (distance / minDist);
                    totalForce += direction * Mathf.Abs(forceValue) * repulsionEffector * dampening * scale;
                }
                // Attractive force calculation.
                if (distance < interactRadius)
                {
                    float scale = 1f - (distance / interactRadius);
                    totalForce += direction * forceValue * dampening * scale;
                }
            }
        }

        // Update velocity and position.
        self.velocity += totalForce * deltaTime;
        self.velocity *= friction;
        self.position += self.velocity * deltaTime;

        // Reintegrate world-space wrapping.
        if (self.position.x < -halfScreenSpace.x)
            self.position.x = halfScreenSpace.x;
        else if (self.position.x > halfScreenSpace.x)
            self.position.x = -halfScreenSpace.x;

        if (self.position.y < -halfScreenSpace.y)
            self.position.y = halfScreenSpace.y;
        else if (self.position.y > halfScreenSpace.y)
            self.position.y = -halfScreenSpace.y;

        outputParticles[i] = self;
    }
}

// A simple data structure for particle state. Must be blittable.
public struct ParticleData
{
    public Vector3 position;
    public Vector3 velocity;
    public int type;
}
