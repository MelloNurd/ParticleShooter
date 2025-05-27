using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

// Struct to represent a spatial grid cell
public struct GridCell
{
    public int StartIndex;
    public int Count;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial struct ParticleMovementSystem : ISystem
{
    private NativeArray<Particle> _cachedParticles;
    private int _lastParticleCount;

    // Spatial grid data structures
    private NativeArray<GridCell> _grid;
    private NativeArray<int> _particleIndices;
    private int2 _gridDimensions;
    private float _cellSize;

    // Add this field to track window size changes
    private float2 _lastScreenSize;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var settings = SystemAPI.GetSingleton<EntitySimSettings>();
        float2 screen = settings.ScreenSpace;
        float2 halfScreen = settings.HalfScreenSpace;
        float friction = 1 - settings.Friction;
        float dampening = settings.Dampening;
        float repulsion = settings.RepulsionEffector;
        int numTypes = settings.NumberOfTypes;

        // Add the screen size check here
        if (!_lastScreenSize.Equals(screen))
        {
            // Screen size changed, need to reinitialize grid
            InitializeGrid(screen);
            _lastScreenSize = screen;
        }

        var particleQuery = SystemAPI.QueryBuilder().WithAll<Particle>().Build();
        int currentCount = particleQuery.CalculateEntityCount();

        if (!_cachedParticles.IsCreated || _lastParticleCount != currentCount)
        {
            if (_cachedParticles.IsCreated)
                _cachedParticles.Dispose();

            if (_particleIndices.IsCreated)
                _particleIndices.Dispose();

            _cachedParticles = new NativeArray<Particle>(currentCount, Allocator.Persistent);
            _particleIndices = new NativeArray<int>(currentCount, Allocator.Persistent);
            _lastParticleCount = currentCount;

            // Initialize or resize grid if needed
            InitializeGrid(screen);
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
                UnityEngine.Debug.LogWarning("Particle count mismatch when caching particles.");
                break;
            }
        }
        int validCachedParticleCount = index;

        if (validCachedParticleCount == 0) // No particles to process
        {
            return;
        }

        // Build spatial grid
        BuildSpatialGrid(validCachedParticleCount, screen);

        Entity forcesEntity = settings.ForcesBuffer;
        Entity minDistsEntity = settings.MinDistancesBuffer;
        Entity radiiEntity = settings.RadiiBuffer;

        // Validate entities before attempting to get buffers
        bool forcesEntityIsValid = state.EntityManager.Exists(forcesEntity) && state.EntityManager.HasComponent<FloatBuffer>(forcesEntity);
        bool minDistEntityIsValid = state.EntityManager.Exists(minDistsEntity) && state.EntityManager.HasComponent<FloatBuffer>(minDistsEntity);
        bool radiiEntityIsValid = state.EntityManager.Exists(radiiEntity) && state.EntityManager.HasComponent<FloatBuffer>(radiiEntity);

        if (!forcesEntityIsValid || !minDistEntityIsValid || !radiiEntityIsValid)
        {
            return;
        }

        NativeArray<FloatBuffer> forcesBufferArray = SystemAPI.GetBuffer<FloatBuffer>(forcesEntity).ToNativeArray(Allocator.TempJob);
        NativeArray<FloatBuffer> minDistsBufferArray = SystemAPI.GetBuffer<FloatBuffer>(minDistsEntity).ToNativeArray(Allocator.TempJob);
        NativeArray<FloatBuffer> radiiBufferArray = SystemAPI.GetBuffer<FloatBuffer>(radiiEntity).ToNativeArray(Allocator.TempJob);

        // Determine max interaction distance for neighbor search
        float maxInteractionRadius = FindMaxInteractionRadius(radiiBufferArray, minDistsBufferArray);

        var job = new ParticleMovementJob
        {
            AllParticles = _cachedParticles.GetSubArray(0, validCachedParticleCount),
            Grid = _grid,
            ParticleIndices = _particleIndices,
            GridDimensions = _gridDimensions,
            CellSize = _cellSize,
            MaxInteractionRadius = maxInteractionRadius,
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

    private void InitializeGrid(float2 screen)
    {
        // Base cell size on screen dimensions but ensure cells aren't too small
        // for reasonable interaction distances (usually min distances or radii)
        _cellSize = math.min(screen.x, screen.y) / 20f;  // Make cells smaller for better precision

        // Ensure grid is recreated when screen size changes
        _gridDimensions = new int2(
            math.max(1, (int)math.ceil(screen.x / _cellSize)),
            math.max(1, (int)math.ceil(screen.y / _cellSize))
        );

        if (_grid.IsCreated)
            _grid.Dispose();

        // Create grid cells
        int totalCells = _gridDimensions.x * _gridDimensions.y;
        _grid = new NativeArray<GridCell>(totalCells, Allocator.Persistent);
    }

    private float FindMaxInteractionRadius(NativeArray<FloatBuffer> radiiBufferArray, NativeArray<FloatBuffer> minDistsBufferArray)
    {
        float maxRadius = 0f;
        float maxMinDist = 0f;

        // Find both maximum radius and maximum minimum distance
        for (int i = 0; i < radiiBufferArray.Length; i++)
        {
            maxRadius = math.max(maxRadius, radiiBufferArray[i].Value);

            if (i < minDistsBufferArray.Length)
                maxMinDist = math.max(maxMinDist, minDistsBufferArray[i].Value);
        }

        // Use the larger of the two for cell calculations
        return math.max(maxRadius, maxMinDist);
    }

    private void BuildSpatialGrid(int particleCount, float2 screen)
    {
        // Reset grid cells
        for (int i = 0; i < _grid.Length; i++)
        {
            _grid[i] = new GridCell { StartIndex = -1, Count = 0 };
        }

        // First pass: count particles per cell
        var cellCounts = new NativeArray<int>(_grid.Length, Allocator.Temp);

        for (int i = 0; i < particleCount; i++)
        {
            int cellIndex = GetCellIndex(_cachedParticles[i].Position, screen);
            if (cellIndex >= 0 && cellIndex < _grid.Length)
            {
                cellCounts[cellIndex]++;
            }
        }

        // Second pass: calculate start indices
        int currentIndex = 0;
        for (int i = 0; i < _grid.Length; i++)
        {
            if (cellCounts[i] > 0)
            {
                _grid[i] = new GridCell { StartIndex = currentIndex, Count = 0 };
                currentIndex += cellCounts[i];
            }
        }

        // Third pass: place particles in the grid
        // Third pass: place particles in the grid
        for (int i = 0; i < particleCount; i++)
        {
            int cellIndex = GetCellIndex(_cachedParticles[i].Position, screen);
            if (cellIndex >= 0 && cellIndex < _grid.Length)
            {
                // Get the cell
                GridCell cell = _grid[cellIndex];

                if (cell.StartIndex >= 0)
                {
                    _particleIndices[cell.StartIndex + cell.Count] = i;
                    cell.Count++;
                    // Update the cell in the array
                    _grid[cellIndex] = cell;
                }
            }
        }


        cellCounts.Dispose();
    }

    private int GetCellIndex(float3 position, float2 screen)
    {
        // Adjust for screen wrapping
        float2 pos = new float2(position.x, position.y);

        // Transform from [-halfScreen, halfScreen] to [0, screen]
        pos += screen * 0.5f;

        // Ensure position is within bounds after wrapping
        pos = math.fmod(pos, screen);
        if (pos.x < 0) pos.x += screen.x;
        if (pos.y < 0) pos.y += screen.y;

        int x = (int)(pos.x / _cellSize);
        int y = (int)(pos.y / _cellSize);

        // Clamp to ensure within bounds
        x = math.clamp(x, 0, _gridDimensions.x - 1);
        y = math.clamp(y, 0, _gridDimensions.y - 1);

        return y * _gridDimensions.x + x;
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_cachedParticles.IsCreated)
            _cachedParticles.Dispose();

        if (_particleIndices.IsCreated)
            _particleIndices.Dispose();

        if (_grid.IsCreated)
            _grid.Dispose();
    }

    [BurstCompile]
    public partial struct ParticleMovementJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Particle> AllParticles;
        [ReadOnly] public NativeArray<GridCell> Grid;
        [ReadOnly] public NativeArray<int> ParticleIndices;
        [ReadOnly] public NativeArray<FloatBuffer> Forces;
        [ReadOnly] public NativeArray<FloatBuffer> MinDistances;
        [ReadOnly] public NativeArray<FloatBuffer> Radii;

        public int2 GridDimensions;
        public float CellSize;
        public float MaxInteractionRadius;
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

            // Get the cell indices to check (current cell + neighbors)
            int cellRadius = math.max(1, (int)math.ceil(MaxInteractionRadius / CellSize));

            // Get the cell for the current particle
            int particleCellX = (int)((particle.Position.x + HalfScreen.x) / CellSize);
            int particleCellY = (int)((particle.Position.y + HalfScreen.y) / CellSize);

            // Check neighboring cells including the current cell
            for (int offsetY = -cellRadius; offsetY <= cellRadius; offsetY++)
            {
                for (int offsetX = -cellRadius; offsetX <= cellRadius; offsetX++)
                {
                    int cellX = particleCellX + offsetX;
                    int cellY = particleCellY + offsetY;

                    // Handle wrapping for grid cells
                    while (cellX < 0) cellX += GridDimensions.x;
                    while (cellX >= GridDimensions.x) cellX -= GridDimensions.x;
                    while (cellY < 0) cellY += GridDimensions.y;
                    while (cellY >= GridDimensions.y) cellY -= GridDimensions.y;

                    int cellIndex = cellY * GridDimensions.x + cellX;
                    if (cellIndex >= 0 && cellIndex < Grid.Length)
                    {
                        GridCell cell = Grid[cellIndex];
                        if (cell.StartIndex >= 0 && cell.Count > 0)
                        {
                            // Process particles in this cell
                            for (int i = 0; i < cell.Count; i++)
                            {
                                int otherIndex = ParticleIndices[cell.StartIndex + i];
                                var other = AllParticles[otherIndex];

                                // Skip self
                                if (other.Position.Equals(particle.Position) && other.Type == particle.Type) continue;

                                float3 dir = other.Position - particle.Position;

                                // Apply screen wrapping
                                if (dir.x > HalfScreen.x) dir.x -= Screen.x;
                                if (dir.x < -HalfScreen.x) dir.x += Screen.x;
                                if (dir.y > HalfScreen.y) dir.y -= Screen.y;
                                if (dir.y < -HalfScreen.y) dir.y += Screen.y;

                                float dist = math.length(dir);
                                if (dist == 0f) continue;

                                // Look up particle interaction parameters
                                int typeIndex = particle.Type * NumTypes + other.Type;
                                if (typeIndex < 0 || typeIndex >= Forces.Length ||
                                    typeIndex >= MinDistances.Length || typeIndex >= Radii.Length)
                                {
                                    continue;
                                }

                                float force = Forces[typeIndex].Value;
                                float minDist = MinDistances[typeIndex].Value;
                                float radius = Radii[typeIndex].Value;

                                // Early exit if particles are beyond interaction range
                                if (dist > radius && dist > minDist) continue;

                                dir = math.normalize(dir);

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
                        }
                    }
                }
            }

            // Update particle position and velocity
            particle.Velocity += totalForce * DeltaTime;
            particle.Velocity *= Friction;
            particle.Position += particle.Velocity * DeltaTime;

            // Apply screen wrapping
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
