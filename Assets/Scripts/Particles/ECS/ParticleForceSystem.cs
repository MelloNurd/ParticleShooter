using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ParticleForceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        var particleQuery = SystemAPI.QueryBuilder().WithAll<ParticleData, VelocityData>().Build();
        var particles = particleQuery.ToComponentDataArray<ParticleData>(Allocator.Temp);
        var velocities = particleQuery.ToComponentDataArray<VelocityData>(Allocator.Temp);

        foreach (var (particle, velocity, entity) in SystemAPI.Query<RefRW<ParticleData>, RefRW<VelocityData>>().WithEntityAccess())
        {
            float3 force = float3.zero;
            float3 position = particle.ValueRO.Position;
            int type = particle.ValueRO.Type;

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].Position.Equals(position)) continue;

                float3 dir = particles[i].Position - position;
                float dist = math.length(dir);
                if (dist == 0f) continue;

                dir = math.normalize(dir);

                // Sample interaction force — adjust as needed
                float interactionForce = 5.0f / (dist * dist);
                force += dir * interactionForce;
            }

            velocity.ValueRW.Velocity += force * deltaTime;
            particle.ValueRW.Position += velocity.ValueRW.Velocity * deltaTime;
        }

        // Apply position to rendering transform
        foreach (var (particle, transform) in SystemAPI.Query<RefRO<ParticleData>, RefRW<LocalTransform>>())
        {
            transform.ValueRW.Position = particle.ValueRO.Position;
        }
    }
}
