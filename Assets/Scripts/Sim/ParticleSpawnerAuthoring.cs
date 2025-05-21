using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public struct ParticleSpawner : IComponentData
{
    public Entity ParticlePrefab;
}

public struct FloatBuffer : IBufferElementData
{
    public float Value;
}

[MaterialProperty("_BaseColor")]
public struct URPMaterialPropertyBaseColor : IComponentData
{
    public float4 Value;
}

public class ParticleSpawnerAuthoring : MonoBehaviour
{
    public GameObject particlePrefab;

    class Baker : Baker<ParticleSpawnerAuthoring>
    {
        public override void Bake(ParticleSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ParticleSpawner
            {
                ParticlePrefab = GetEntity(authoring.particlePrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial class ParticleSpawnerSystem : SystemBase
{
    private bool _hasSpawned;

    protected override void OnCreate()
    {
        RequireForUpdate<SimSettings>();
        RequireForUpdate<ParticleSpawner>();
        // We don't require RespawnParticles for update because it might not 
        // exist when the simulation first starts
        _hasSpawned = false;
    }

    protected override void OnUpdate()
    {
        // Check for respawn flag
        bool shouldRespawn = false;
        EntityQuery respawnQuery = EntityManager.CreateEntityQuery(typeof(RespawnParticles));

        if (respawnQuery.CalculateEntityCount() > 0)
        {
            Debug.Log("Detected respawn signal - respawning particles");
            shouldRespawn = true;
            // We'll remove the component from all entities with the tag
            // (should just be the one flag entity)
            EntityManager.RemoveComponent<RespawnParticles>(respawnQuery);
            _hasSpawned = false;
        }

        // Only continue if we haven't spawned yet or need to respawn
        if (_hasSpawned && !shouldRespawn)
            return;

        // Get singleton data (must be baked from authoring)
        var settings = SystemAPI.GetSingleton<SimSettings>();
        var spawner = SystemAPI.GetSingleton<ParticleSpawner>();

        Debug.Log($"Spawning {settings.NumberOfParticles} particles with {settings.NumberOfTypes} types");

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
        Debug.Log("Particle spawning complete");
    }
}
