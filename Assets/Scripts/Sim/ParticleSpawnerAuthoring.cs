using Unity.Entities;
using UnityEngine;

public struct ParticleSpawner : IComponentData
{
    public Entity ParticlePrefab;
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
