using Unity.Entities;
using Unity.Mathematics;

public struct SimSettings : IComponentData
{
    public int NumberOfParticles;
    public int NumberOfTypes;
    public float Friction;
    public float Dampening;
    public float RepulsionEffector;
    public float2 ScreenSpace;
    public float2 HalfScreenSpace;

    public Entity ForcesBuffer;
    public Entity MinDistancesBuffer;
    public Entity RadiiBuffer;
}

public struct FloatBuffer : IBufferElementData
{
    public float Value;
}