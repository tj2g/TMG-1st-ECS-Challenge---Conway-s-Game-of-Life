using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigComponent>();
        state.RequireForUpdate<CellRandom>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Only running once to set up the grid of cells
        state.Enabled = false;

        var cellConfig = SystemAPI.GetSingleton<ConfigComponent>();
        var random = SystemAPI.GetSingleton<CellRandom>();
        Random rand = random.Value;

        int columns = cellConfig.Columns;
        int rows = cellConfig.Rows;
        int totalCells = columns * rows;

        var rotation = quaternion.RotateX(0);
        var cellEntity = state.EntityManager.Instantiate(cellConfig.CellPrefab, totalCells, state.WorldUpdateAllocator);

        // Setting up starting count of cells
        int i = 0;

        // Building the indexes and coordinates
        for (int x = 0; x < rows; x++) // rows
        {
            for (int y = 0; y < columns; y++) // columns
            {
                var cell = cellEntity[i];

                bool alive = rand.NextBool();//false;

                // Setting Information and location for the individual cell
                SystemAPI.SetComponent(cell, new CellComponent
                {
                    Index = i,
                    ColumnPos = y,
                    RowPos = x,
                    IsAlive = alive,
                });

                // Actually positioning the individual cell
                SystemAPI.SetComponent(cell, new LocalTransform
                {
                    Position = new float3(y, x, 0),
                    Rotation = rotation,
                    Scale = 0.95f
                });

                i++;
            }
        }
    }
}