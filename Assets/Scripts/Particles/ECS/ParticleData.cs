using Unity.Entities;
using Unity.Mathematics;

public struct ParticleData : IComponentData
{
    public float3 Position;
    public int Type;
}

public struct VelocityData : IComponentData
{
    public float3 Velocity;
}
