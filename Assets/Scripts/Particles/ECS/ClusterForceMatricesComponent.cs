using Unity.Entities;
using Unity.Mathematics;

public struct ClusterForceMatricesComponent : IComponentData
{
    public Entity ClusterOwner;
    public int ClusterId;
    public BlobAssetReference<ForceMatricesBlob> ForceData;
    public float3 Center;
    public float MaxExternalRadii;
}

public struct ForceMatricesBlob
{
    public BlobArray<float> InternalForces;    // [type1 * typeCount + type2]
    public BlobArray<float> ExternalForces;    // [type1 * (typeCount+2) + type2]
    public BlobArray<float> InternalRadii;
    public BlobArray<float> ExternalRadii;
    public BlobArray<float> InternalMins;
    public BlobArray<float> ExternalMins;
    public int TypeCount;
}