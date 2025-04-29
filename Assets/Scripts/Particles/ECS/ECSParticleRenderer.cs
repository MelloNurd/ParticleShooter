using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class ECSParticleRenderer : MonoBehaviour
{
    public Mesh mesh;         // assign a sphere or quad
    public Material material; // any unlit URP material

    EntityManager em;

    void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (mesh == null || material == null) return;

        var query = em.CreateEntityQuery(typeof(ParticleData));
        var particles = query.ToComponentDataArray<ParticleData>(Unity.Collections.Allocator.Temp);

        List<Matrix4x4> matrices = new();
        foreach (var p in particles)
        {
            matrices.Add(Matrix4x4.TRS(p.Position, Quaternion.identity, Vector3.one * 0.2f));
        }

        if (matrices.Count > 0)
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices);

        particles.Dispose();
    }
}
