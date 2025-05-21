using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public struct Particle : IComponentData
{
    public float3 Position;
    public float3 Velocity;
    public int Type;
}

public class ParticleAuthoring : MonoBehaviour
{
    public int Type;

    class Baker : Baker<ParticleAuthoring>
    {
        public override void Bake(ParticleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable); // instead of Dynamic

            AddComponent(entity, new Particle
            {
                Position = authoring.transform.position,
                Velocity = float3.zero,
                Type = authoring.Type
            });
        }
    }
}
