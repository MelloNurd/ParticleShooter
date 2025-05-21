using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial class ParticleSpawnerSystem : SystemBase
{
    private bool _hasSpawned;

    protected override void OnCreate()
    {
        RequireForUpdate<SimSettings>();
        _hasSpawned = false;
    }

    protected override void OnUpdate()
    {
        if (_hasSpawned)
            return;

        // Get singleton data (must be baked from authoring)
        var settings = SystemAPI.GetSingleton<SimSettings>();
        var spawner = SystemAPI.GetSingleton<ParticleSpawner>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var rand = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

        for (int i = 0; i < settings.NumberOfParticles; i++)
        {
            Entity particleEntity = ecb.Instantiate(spawner.ParticlePrefab);

            float3 position = new float3(
                rand.NextFloat(-settings.HalfScreenSpace.x, settings.HalfScreenSpace.x),
                rand.NextFloat(-settings.HalfScreenSpace.y, settings.HalfScreenSpace.y),
                0f);

            int type = rand.NextInt(0, settings.NumberOfTypes);

            ecb.SetComponent(particleEntity, new Particle
            {
                Position = position,
                Velocity = float3.zero,
                Type = type
            });

            ecb.SetComponent(particleEntity, LocalTransform.FromPosition(position));

            float hue = (float)type / settings.NumberOfTypes;
            UnityEngine.Color rgb = UnityEngine.Color.HSVToRGB(hue, 1f, 1f);

            ecb.AddComponent(particleEntity, new URPMaterialPropertyBaseColor
            {
                Value = new float4(rgb.r, rgb.g, rgb.b, 1f)
            });
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        _hasSpawned = true;
    }
}
