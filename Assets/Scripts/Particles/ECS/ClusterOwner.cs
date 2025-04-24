using Unity.Entities;

public struct ClusterParticleTag : IComponentData { } // Just tags a particle
public struct ClusterOwner : IComponentData
{
    public Entity ClusterEntity;
    public int ClusterId;
}