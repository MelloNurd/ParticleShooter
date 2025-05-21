using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = Unity.Mathematics.Random;
public struct ParticleSpawnData : IComponentData
{
    public float2 ScreenSize;
    public float2 HalfScreenSize;

    public Entity ParticlePrefab;
    public int NumberOfParticles;
    public int NumberOfTypes;

    public float Repulsion;
    public float Dampening;
    public float Friction;

    public Random random;
}

public class ParticleSpawnerAuthoring : MonoBehaviour
{
    public Vector2 ScreenSize = new Vector2(128, 74);

    public GameObject ParticlePrefab;
    public int NumberOfParticles;
    public int NumberOfTypes;
    
    public float Repulsion;
    public float Dampening;
    public float Friction;

    private class Baker : Baker<ParticleSpawnerAuthoring>
    {
        public override void Bake(ParticleSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            ParticleSpawnData particleSpawnData = new ParticleSpawnData
            {
                ScreenSize = authoring.ScreenSize,
                HalfScreenSize = authoring.ScreenSize * 0.5f,

                ParticlePrefab = GetEntity(authoring.ParticlePrefab, TransformUsageFlags.None),
                NumberOfParticles = authoring.NumberOfParticles,
                NumberOfTypes = authoring.NumberOfTypes,

                Repulsion = authoring.Repulsion,
                Dampening = authoring.Dampening,
                Friction = authoring.Friction,
            };
            AddComponent(entity, particleSpawnData);
        }
    }
}

public partial struct ParticleSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ParticleSpawnData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var spawner = SystemAPI.GetSingleton<ParticleSpawnData>();

        spawner.random = new Random((uint)(SystemAPI.Time.ElapsedTime * 100) + 1);

        int count = spawner.NumberOfTypes * spawner.NumberOfTypes;

        for (int i = 0; i < spawner.NumberOfParticles; i++)
        {
            Entity newParticle = ecb.Instantiate(spawner.ParticlePrefab);

            // 3) Now give it a Force buffer:
            var fb = ecb.AddBuffer<ForceElement>(newParticle);
            fb.ResizeUninitialized(count);
            for (int j = 0; j < count; j++)
                fb[j] = new ForceElement { Value = spawner.random.NextFloat(1, 4) };

            // 4) And a MinDist buffer:
            var mb = ecb.AddBuffer<MinDistElement>(newParticle);
            mb.ResizeUninitialized(count);
            for (int j = 0; j < count; j++)
                mb[j] = new MinDistElement { Value = spawner.random.NextFloat(6, 12) };

            // 5) And a Radii buffer:
            var rb = ecb.AddBuffer<RadiusElement>(newParticle);
            rb.ResizeUninitialized(count);
            for (int j = 0; j < count; j++)
                rb[j] = new RadiusElement { Value = spawner.random.NextFloat(10, 18) };

            ecb.SetComponent(newParticle, LocalTransform.FromPosition(Utilities.GetRandomPointOnScreen()));

            state.Enabled = false;
        }
    }
}