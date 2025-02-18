using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.Runtime.InteropServices;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;

public class ParticleJobManager : MonoBehaviour
{
    // Interaction arrays (flattened, size: numTypes * numTypes)
    public float[] forces;
    public float[] minDistances;
    public float[] radii;

    // References to Particle GameObjects (populated by your ParticleManager)
    private List<int> particlesToRemove = new List<int>();
    //private Particle[] particles;

    // Native arrays for simulation data and interaction parameters.
    // Double-buffered arrays:
    private NativeArray<ParticleJobData> particleArrayRead;
    private NativeArray<ParticleJobData> particleArrayWrite;
    private NativeArray<float> nativeForces;
    private NativeArray<float> nativeMinDistances;
    private NativeArray<float> nativeRadii;

    // Job handle for scheduling.
    private JobHandle jobHandle;

    private bool _isReady;

    ParticleSystem.Particle[] particles;

    public void AssignTables(float[,] forcesMatrix, float[,] minDistancesMatrix, float[,] radiiMatrix)
    {
        int numTypes = ParticleManagerJOBS.Instance.NumberOfTypes;
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
        Debug.Log($"Initializing Jobs with {ParticleManagerJOBS.Particles.Count} particles.");

        // Get the particles from your ParticleManager.
        //particles = ParticleManager.Particles.ToArray();

        particles = new ParticleSystem.Particle[ParticleManagerJOBS.Instance.NumberOfParticles];

        int numParticles = ParticleManagerJOBS.Instance.ParticleSystem.GetParticles(particles);

        // Allocate two NativeArrays for double buffering.
        particleArrayRead = new NativeArray<ParticleJobData>(numParticles, Allocator.Persistent);
        particleArrayWrite = new NativeArray<ParticleJobData>(numParticles, Allocator.Persistent);

        // Initialize both buffers with the current particle data.
        for (int i = 0; i < numParticles; i++)
        {
            ParticleJobData pd = new ParticleJobData
            {
                position = particles[i].position,
                velocity = Vector3.zero,
                type = ParticleManagerJOBS.Particles[i].Type,
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
            //RemoveParticles();
        }

        int numParticles = particleArrayRead.Length;

        // Set up and schedule the job using the double-buffered arrays.
        ParticleJob job = new ParticleJob
        {
            inputParticles = particleArrayRead,
            outputParticles = particleArrayWrite,
            forces = nativeForces,
            minDistances = nativeMinDistances,
            radii = nativeRadii,
            numParticles = numParticles,
            numTypes = ParticleManagerJOBS.Instance.NumberOfTypes,
            deltaTime = Time.deltaTime,
            friction = ParticleManagerJOBS.Instance.Friction,
            dampening = ParticleManagerJOBS.Instance.Dampening,
            repulsionEffector = ParticleManagerJOBS.Instance.RepulsionEffector,
            // Pass screen parameters for world wrapping.
            screenSpace = ParticleManagerJOBS.Instance.ScreenSpace,         // For example: (ScreenSpace.x, ScreenSpace.y)
            halfScreenSpace = ParticleManagerJOBS.Instance.HalfScreenSpace      // For example: (HalfScreenSpace.x, HalfScreenSpace.y)
        };

        // Schedule with a batch size (64 particles per batch).
        jobHandle = job.Schedule(numParticles, 256);
        jobHandle.Complete();

        // Update the GameObjects with the new positions.
        for (int i = 0; i < numParticles; i++)
        {
            //particles[i].transform.position = particleArrayWrite[i].position;
            particles[i].position = particleArrayWrite[i].position;
        }

        ParticleManagerJOBS.Instance.ParticleSystem.SetParticles(particles, numParticles);

        // Swap the buffers so the output becomes the input for the next frame.
        NativeArray<ParticleJobData> temp = particleArrayRead;
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

    //public void RequestParticleRemoval(Particle particle)
    //{
    //    int index = System.Array.IndexOf(particles, particle);
    //    if (index >= 0 && !particlesToRemove.Contains(index))
    //    {
    //        particlesToRemove.Add(index);
    //    }
    //}

    //private void RemoveParticles()
    //{
    //    // Sort indices in descending order to avoid reindexing issues
    //    particlesToRemove.Sort((a, b) => b.CompareTo(a));

    //    foreach (int index in particlesToRemove)
    //    {
    //        // Remove from particles array
    //        var tempList = particles.ToList();
    //        tempList.RemoveAt(index);
    //        particles = tempList.ToArray();

    //        // Remove from native arrays
    //        RemoveAtNativeArray(ref particleArrayRead, index);
    //        RemoveAtNativeArray(ref particleArrayWrite, index);
    //    }

    //    particlesToRemove.Clear();
    //}

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
public struct ParticleJob : IJobParallelFor
{
    // Read-only input buffer.
    [Unity.Collections.ReadOnly] public NativeArray<ParticleJobData> inputParticles;
    // Output buffer where each thread writes its own result.
    public NativeArray<ParticleJobData> outputParticles;

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
        ParticleJobData self = inputParticles[i];
        Vector3 totalForce = Vector3.zero;

        for (int j = 0; j < numParticles; j++)
        {
            if (i == j)
                continue;

            ParticleJobData other = inputParticles[j];

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
public struct ParticleJobData
{
    public Vector3 position;
    public Vector3 velocity;
    public int type;
}
