using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ParticleAuthoring : MonoBehaviour
{
    public int type;
}

public class ParticleBaker : Baker<ParticleAuthoring>
{
    public override void Bake(ParticleAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new ParticleData
        {
            Position = authoring.transform.position,
            Type = authoring.type
        });
        AddComponent(entity, new VelocityData
        {
            Velocity = float3.zero
        });
        AddComponent(entity, new ClusterOwner
        {
            ClusterId = 0 // Default
        });
    }
}