using Unity.Entities;

public class ParticleEntityPrefabBaker : Baker<ParticleEntityPrefabAuthoring>
{
    public override void Bake(ParticleEntityPrefabAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        var prefabEntity = GetEntity(authoring.particlePrefabGO, TransformUsageFlags.Dynamic);
        AddComponent(entity, new ParticleEntityPrefabReference { Prefab = prefabEntity });
    }
}

public struct ParticleEntityPrefabReference : IComponentData
{
    public Entity Prefab;
}
