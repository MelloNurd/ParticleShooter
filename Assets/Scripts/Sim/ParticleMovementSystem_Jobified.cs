using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial struct ParticleMovementSystem_Jobified : ISystem
{
    private NativeArray<Particle> _cachedParticles;
    private int _lastParticleCount;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var settings = SystemAPI.GetSingleton<SimSettings>();
        float2 screen = settings.ScreenSpace;
        float2 halfScreen = settings.HalfScreenSpace;
        float friction = settings.Friction;
        float dampening = settings.Dampening;
        float repulsion = settings.RepulsionEffector;
        int numTypes = settings.NumberOfTypes;

        var particleQuery = SystemAPI.QueryBuilder().WithAll<Particle>().Build();
        int currentCount = particleQuery.CalculateEntityCount();

        if (!_cachedParticles.IsCreated || _lastParticleCount != currentCount)
        {
            if (_cachedParticles.IsCreated)
                _cachedParticles.Dispose();

            _cachedParticles = new NativeArray<Particle>(currentCount, Allocator.Persistent);
            _lastParticleCount = currentCount;
        }

        var index = 0;
        foreach (var p in SystemAPI.Query<RefRO<Particle>>())
        {
            if (index < _cachedParticles.Length)
            {
                _cachedParticles[index++] = p.ValueRO;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Particle count mismatch when caching particles."); // This literal string is Burst-compatible
                break;
            }
        }
        int validCachedParticleCount = index;

        if (validCachedParticleCount == 0) // No particles to process
        {
            return;
        }

        Entity forcesEntity = settings.ForcesBuffer;
        Entity minDistsEntity = settings.MinDistancesBuffer;
        Entity radiiEntity = settings.RadiiBuffer;

        // Validate entities before attempting to get buffers
        bool forcesEntityIsValid = state.EntityManager.Exists(forcesEntity) && state.EntityManager.HasComponent<FloatBuffer>(forcesEntity);
        bool minDistEntityIsValid = state.EntityManager.Exists(minDistsEntity) && state.EntityManager.HasComponent<FloatBuffer>(minDistsEntity);
        bool radiiEntityIsValid = state.EntityManager.Exists(radiiEntity) && state.EntityManager.HasComponent<FloatBuffer>(radiiEntity);

        if (!forcesEntityIsValid || !minDistEntityIsValid || !radiiEntityIsValid)
        {
            // Removed the Burst-incompatible Debug.LogWarning line
            return;
        }

        // Directly get the buffer and convert to NativeArray.
        // If an ObjectDisposedException occurs, it will now propagate from here.
        NativeArray<FloatBuffer> forcesBufferArray = SystemAPI.GetBuffer<FloatBuffer>(forcesEntity).ToNativeArray(Allocator.TempJob);
        NativeArray<FloatBuffer> minDistsBufferArray = SystemAPI.GetBuffer<FloatBuffer>(minDistsEntity).ToNativeArray(Allocator.TempJob);
        NativeArray<FloatBuffer> radiiBufferArray = SystemAPI.GetBuffer<FloatBuffer>(radiiEntity).ToNativeArray(Allocator.TempJob);

        var job = new ParticleMovementJob
        {
            AllParticles = _cachedParticles.GetSubArray(0, validCachedParticleCount),
            Screen = screen,
            HalfScreen = halfScreen,
            Friction = friction,
            Dampening = dampening,
            RepulsionEffector = repulsion,
            DeltaTime = deltaTime,
            NumTypes = numTypes,
            Forces = forcesBufferArray,
            MinDistances = minDistsBufferArray,
            Radii = radiiBufferArray
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        state.Dependency = forcesBufferArray.Dispose(state.Dependency);
        state.Dependency = minDistsBufferArray.Dispose(state.Dependency);
        state.Dependency = radiiBufferArray.Dispose(state.Dependency);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_cachedParticles.IsCreated)
            _cachedParticles.Dispose();
    }

    // ParticleMovementJob struct remains the same
    [BurstCompile]
    public partial struct ParticleMovementJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Particle> AllParticles;
        [ReadOnly] public NativeArray<FloatBuffer> Forces;
        [ReadOnly] public NativeArray<FloatBuffer> MinDistances;
        [ReadOnly] public NativeArray<FloatBuffer> Radii;

        public float2 Screen;
        public float2 HalfScreen;
        public float Friction;
        public float Dampening;
        public float RepulsionEffector;
        public float DeltaTime;
        public int NumTypes;

        public void Execute(ref Particle particle, ref LocalTransform transform)
        {
            float3 totalForce = float3.zero;

            for (int j = 0; j < AllParticles.Length; j++)
            {
                var other = AllParticles[j];
                if (AllParticles[j].Position.Equals(particle.Position) && AllParticles[j].Type == particle.Type) continue;

                float3 dir = other.Position - particle.Position;

                if (dir.x > HalfScreen.x) dir.x -= Screen.x;
                if (dir.x < -HalfScreen.x) dir.x += Screen.x;
                if (dir.y > HalfScreen.y) dir.y -= Screen.y;
                if (dir.y < -HalfScreen.y) dir.y += Screen.y;

                float dist = math.length(dir);
                if (dist == 0f) continue;

                dir = math.normalize(dir);

                int typeIndex = particle.Type * NumTypes + other.Type;
                if (typeIndex < 0 || typeIndex >= Forces.Length || typeIndex >= MinDistances.Length || typeIndex >= Radii.Length)
                {
                    continue;
                }

                float force = Forces[typeIndex].Value;
                float minDist = MinDistances[typeIndex].Value;
                float radius = Radii[typeIndex].Value;

                if (dist < minDist)
                {
                    float scale = 1f - (dist / minDist);
                    totalForce += dir * math.abs(force) * RepulsionEffector * Dampening * scale;
                }

                if (dist < radius)
                {
                    float scale = 1f - (dist / radius);
                    totalForce += dir * force * Dampening * scale;
                }
            }

            particle.Velocity += totalForce * DeltaTime;
            particle.Velocity *= Friction;
            particle.Position += particle.Velocity * DeltaTime;

            float3 pos = particle.Position;
            if (pos.x < -HalfScreen.x) pos.x = HalfScreen.x;
            else if (pos.x > HalfScreen.x) pos.x = -HalfScreen.x;
            if (pos.y < -HalfScreen.y) pos.y = HalfScreen.y;
            else if (pos.y > HalfScreen.y) pos.y = -HalfScreen.y;

            particle.Position = pos;
            transform.Position = pos;
        }
    }
}