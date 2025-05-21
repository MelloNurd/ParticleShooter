using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public struct ParticleVelocity : IComponentData
{
    public float3 Value;
}

public struct ParticleData : IComponentData
{
    public Entity Entity;
    public int Id;
    public int Type;

    public ParticleSpawnData SpawnData;
}

public struct ForceElement : IBufferElementData
{
    public float Value;
}

public struct MinDistElement : IBufferElementData
{
    public float Value;
}

public struct RadiusElement : IBufferElementData
{
    public float Value;
}

public class ParticleAuthoring : MonoBehaviour
{
    public float[] Forces;
    public float[] MinDistances;
    public float[] Radii;

    private class Baker : Baker<ParticleAuthoring>
    {
        public override void Bake (ParticleAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ParticleData
            {
                Entity = entity,
            });
            AddComponent<ParticleVelocity>(entity);
        }
    }
}

public partial struct ParticleMoveSystem : ISystem
{
    EntityQuery _query;
    NativeArray<LocalTransform> _allTransforms;   // ← persistent
    NativeArray<ParticleData> _allDatas;        // ← persistent

    public void OnCreate(ref SystemState state)
    {
        // build your query once
        _query = state.GetEntityQuery(
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadOnly<ParticleData>());

        // allocate both arrays once, with the initial size
        int initialCount = _query.CalculateEntityCount();
        _allTransforms = new NativeArray<LocalTransform>(initialCount, Allocator.Persistent);
        _allDatas = new NativeArray<ParticleData>(initialCount, Allocator.Persistent);

        // make sure we tear them down later
        state.RequireForUpdate(_query);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_allTransforms.IsCreated) _allTransforms.Dispose();
        if (_allDatas.IsCreated) _allDatas.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        // 1) check if entity count changed — if so, resize
        int count = _query.CalculateEntityCount();
        if (_allTransforms.Length != count)
        {
            _allTransforms.Dispose();
            _allDatas.Dispose();
            _allTransforms = new NativeArray<LocalTransform>(count, Allocator.Persistent);
            _allDatas = new NativeArray<ParticleData>(count, Allocator.Persistent);
        }

        // 2) copy into the *existing* arrays (no alloc!)
        _query.CopyComponentDataToArray(_allTransforms);  // fills the array in-place
        _query.CopyComponentDataToArray(_allDatas);       // ditto

        // 3) schedule your job using those arrays (no dispose here)
        var job = new ParticleMovementJob
        {
            allTransforms = _allTransforms,
            allDatas = _allDatas,
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct ParticleMovementJob : IJobEntity
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<LocalTransform> allTransforms;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ParticleData> allDatas;
        [ReadOnly] public float DeltaTime;

        // ref = read/write, in = read only
        public void Execute(
            [EntityIndexInQuery] int idx,
            ref ParticleVelocity velocity,
            ref LocalTransform transform,
            in ParticleData data,
            in DynamicBuffer<ForceElement> forces,
            in DynamicBuffer<MinDistElement> minDists,
            in DynamicBuffer<RadiusElement> radii
        ) {
            float3 totalForce = float3.zero;

            for (int j = 0; j < allTransforms.Length; j++)
            {
                if (idx == j)
                    continue;

                LocalTransform other = allTransforms[j];

                // Calculate direction and apply world wrapping adjustments for distance calculation.
                float3 direction = allTransforms[j].Position - transform.Position;
                if (direction.x > data.SpawnData.HalfScreenSize.x)
                    direction.x -= data.SpawnData.ScreenSize.x;
                if (direction.x < -data.SpawnData.HalfScreenSize.x)
                    direction.x += data.SpawnData.ScreenSize.x;
                if (direction.y > data.SpawnData.HalfScreenSize.y)
                    direction.y -= data.SpawnData.ScreenSize.y;
                if (direction.y < -data.SpawnData.HalfScreenSize.y)
                    direction.y += data.SpawnData.ScreenSize.y;

                float distance = math.length(direction);
                if (distance > 0f)
                {
                    math.normalize(direction);
                    int paramIndex = data.Type * data.SpawnData.NumberOfTypes + allDatas[j].Type;
                    float minDist = minDists[paramIndex].Value;
                    float interactRadius = radii[paramIndex].Value;
                    float forceValue = forces[paramIndex].Value;

                    // Repulsive force calculation.
                    if (distance < minDist)
                    {
                        float scale = 1f - (distance / minDist);
                        totalForce += direction * math.abs(forceValue) * data.SpawnData.Repulsion * data.SpawnData.Dampening * scale;
                    }
                    // Attractive force calculation.
                    if (distance < interactRadius)
                    {
                        float scale = 1f - (distance / interactRadius);
                        totalForce += direction * forceValue * data.SpawnData.Dampening * scale;
                    }
                }
            }

            // Update velocity and position.
            velocity.Value += totalForce * DeltaTime;
            velocity.Value *= data.SpawnData.Friction;
            transform.Position += velocity.Value * DeltaTime;

            // Reintegrate world-space wrapping.
            if (transform.Position.x < -data.SpawnData.HalfScreenSize.x)
                transform.Position.x = data.SpawnData.HalfScreenSize.x;
            else if (transform.Position.x > data.SpawnData.HalfScreenSize.x)
                transform.Position.x = -data.SpawnData.HalfScreenSize.x;

            if (transform.Position.y < -data.SpawnData.HalfScreenSize.y)
                transform.Position.y = data.SpawnData.HalfScreenSize.y;
            else if (transform.Position.y > data.SpawnData.HalfScreenSize.y)
                transform.Position.y = -data.SpawnData.HalfScreenSize.y;
        }
    }
}