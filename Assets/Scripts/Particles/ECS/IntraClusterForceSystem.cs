using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct IntraClusterForceSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        // For each cluster's force blob...
        foreach (var (forceData, _) in SystemAPI.Query<RefRO<ClusterForceMatricesComponent>>().WithEntityAccess())
        {
            int clusterId = forceData.ValueRO.ClusterId;
            ref var blob = ref forceData.ValueRO.ForceData.Value;

            // Query only particles in this cluster
            var particleQuery = SystemAPI.QueryBuilder()
                .WithAll<ParticleData, VelocityData, ClusterOwner>()
                .Build();

            var particleData = particleQuery.ToComponentDataArray<ParticleData>(Allocator.Temp);
            var velocityData = particleQuery.ToComponentDataArray<VelocityData>(Allocator.Temp);
            var ownerData = particleQuery.ToComponentDataArray<ClusterOwner>(Allocator.Temp);
            var entityArray = particleQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < particleData.Length; i++)
            {
                if (ownerData[i].ClusterId != clusterId) continue;

                float3 position = particleData[i].Position;
                int type = particleData[i].Type;
                float3 force = float3.zero;

                for (int j = 0; j < particleData.Length; j++)
                {
                    if (i == j || ownerData[j].ClusterId != clusterId) continue;

                    float3 otherPos = particleData[j].Position;
                    int otherType = particleData[j].Type;

                    float3 dir = otherPos - position;
                    float dist = math.length(dir);
                    if (dist == 0f) continue;

                    dir = math.normalize(dir);
                    int index = type * blob.TypeCount + otherType;
                    float attraction = blob.InternalForces[index];
                    float radius = blob.InternalRadii[index];

                    if (dist < radius)
                    {
                        float weight = math.saturate(1 - dist / radius);
                        force += dir * attraction * weight;
                    }
                }

                var v = velocityData[i];
                v.Velocity += force * dt;
                velocityData[i] = v;

                var p = particleData[i];
                p.Position += v.Velocity * dt;
                particleData[i] = p;
            }

            // Write results
            for (int i = 0; i < particleData.Length; i++)
            {
                SystemAPI.SetComponent(entityArray[i], particleData[i]);
                SystemAPI.SetComponent(entityArray[i], velocityData[i]);
            }

            particleData.Dispose();
            velocityData.Dispose();
            ownerData.Dispose();
            entityArray.Dispose();
        }
    }
}
