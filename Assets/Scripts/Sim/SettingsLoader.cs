using NaughtyAttributes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

public class SettingsLoader : MonoBehaviour
{
    [Header("Simulation Configuration")] ////////////////////////////////////////////////////////////////
    public Vector2 screenSpace = new Vector2(32, 18);
    [UnityEngine.Range(1, 9999)] public int numberOfParticles = 1000;
    [UnityEngine.Range(1, 32)] public int numberOfTypes = 5;

    [Header("Particle Properties")] /////////////////////////////////////////////////////////////////////
    [MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _forcesRange = new Vector2(0.3f, 1f);
    [MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _minDistancesRange = new Vector2(1f, 3f);
    [MinMaxSlider(0.0f, 18.0f)][SerializeField] private Vector2 _radiiRange = new Vector2(3f, 5f);
    
    [Space(10)]
    [UnityEngine.Range(-5, 5)] public float repulsion = -5f;       // Increased repulsion strength from -2f
    [UnityEngine.Range(0, 2)] public float friction = 0.95f;      // Higher friction to maintain momentum (less slowdown)
    [UnityEngine.Range(0, 1)] public float dampening = 0.5f;      // Increased from 0.05f to amplify force effects

    [Header("Unity Settings")] /////////////////////////////////////////////////////////////////////
    [UnityEngine.Range(0, 5)][SerializeField] public float _timeScale = 1f;

    void Start()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity forcesEntity = em.CreateEntity(typeof(FloatBuffer));
        Entity minDistEntity = em.CreateEntity(typeof(FloatBuffer));
        Entity radiiEntity = em.CreateEntity(typeof(FloatBuffer));

        Entity simSettingsEntity = em.CreateEntity();
        em.AddComponentData(simSettingsEntity, new SimSettings
        {
            NumberOfParticles = numberOfParticles,
            NumberOfTypes = numberOfTypes,
            Friction = friction,
            Dampening = dampening,
            RepulsionEffector = repulsion,
            ScreenSpace = screenSpace,
            HalfScreenSpace = screenSpace * 0.5f,
            ForcesBuffer = forcesEntity,
            MinDistancesBuffer = minDistEntity,
            RadiiBuffer = radiiEntity
        });

        var forces = em.GetBuffer<FloatBuffer>(forcesEntity);
        var minDists = em.GetBuffer<FloatBuffer>(minDistEntity);
        var radii = em.GetBuffer<FloatBuffer>(radiiEntity);

        int size = numberOfTypes * numberOfTypes;
        for (int i = 0; i < size; i++)
        {
            // Much stronger forces: range is now -3 to 3 instead of -1 to 1
            float force = UnityEngine.Random.Range(_forcesRange.x, _forcesRange.y);

            // Slightly smaller minimum distances for tighter interactions
            float minDist = UnityEngine.Random.Range(_minDistancesRange.x, _minDistancesRange.y);

            // Larger interaction radii for more frequent interactions
            float radius = UnityEngine.Random.Range(_radiiRange.x, _radiiRange.y);

            forces.Add(new FloatBuffer { Value = force });
            minDists.Add(new FloatBuffer { Value = minDist });
            radii.Add(new FloatBuffer { Value = radius });
        }
    }
}