using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

[UpdateAfter(typeof(SpawnSystem))]
[BurstCompile]
public partial struct SettingNeighborsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Running once to set up the neighbors
        state.Enabled = false;

        var config = SystemAPI.GetSingleton<ConfigComponent>();
        int totalCount = config.Columns * config.Rows;
        int columns = config.Columns;
        var cellIndex = CollectionHelper.CreateNativeArray<int>(totalCount, state.WorldUpdateAllocator);
        var cellColumnPos = CollectionHelper.CreateNativeArray<int>(totalCount, state.WorldUpdateAllocator);
        var cellRowPos = CollectionHelper.CreateNativeArray<int>(totalCount, state.WorldUpdateAllocator);

        var getCurrentIndex = new GetCurrentIndex
        {
            cellIndex = cellIndex,
        }.ScheduleParallel(state.Dependency);

        getCurrentIndex.Complete();

        var getCurrentColumnPos = new GetCurrentColumnPos
        {
            cellColumnPos = cellColumnPos,
        }.ScheduleParallel(state.Dependency);

        getCurrentColumnPos.Complete();

        var getRowPos = new GetCurrentRowPos
        {
            cellRowPos = cellRowPos,
        }.ScheduleParallel(state.Dependency);

        getRowPos.Complete();

        
        var settingNeighborCells = new SettingNeighborCells
        {
            cellIndex = cellIndex.AsReadOnly(),
            cellColumnPos = cellColumnPos.AsReadOnly(),
            cellRowPos = cellRowPos.AsReadOnly(),
            totalCount = totalCount,
            columns = columns,
        }.ScheduleParallel(state.Dependency);

        settingNeighborCells.Complete();

    }
}


[BurstCompile]
public partial struct SettingNeighborCells : IJobEntity
{
    [NativeDisableParallelForRestriction]
    [ReadOnly] public NativeArray<int>.ReadOnly cellIndex;
    [ReadOnly] public NativeArray<int>.ReadOnly cellColumnPos;
    [ReadOnly] public NativeArray<int>.ReadOnly cellRowPos;
    public int totalCount;
    public int columns;
    public void Execute(in CellComponent cellComponent, DynamicBuffer<NeighborCell> neighborCells)
    {
        var tempIndex = new NativeArray<int>(8, Allocator.Temp);
        int countBuffers = 0;
        // Bottom Left = 0
        {
            if (cellComponent.Index - columns - 1 < 0)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.ColumnPos - 1 != cellColumnPos[cellComponent.Index - columns - 1] &&
                    cellComponent.RowPos - 1 != cellRowPos[cellComponent.Index - columns - 1])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index - columns - 1;
                }
            }
        }

        // Bottom neighbor = 1
        {
            if (cellComponent.Index - columns < 0)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.RowPos - 1 != cellRowPos[cellComponent.Index - columns])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index - columns;
                }
            }
        }

        // Bottom right neigbor = 2
        {
            if (cellComponent.Index - columns + 1 < 0)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.ColumnPos + 1 != cellColumnPos[cellComponent.Index - columns + 1] &&
                    cellComponent.RowPos - 1 != cellRowPos[cellComponent.Index - columns + 1])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index - columns + 1;
                }
            }
        }

        // Left neighbor = 3
        {
            if (cellComponent.Index - 1 < 0)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.ColumnPos - 1 != cellColumnPos[cellComponent.Index - 1])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index - 1;
                }
            }
        }

        // Right neighbor = 4
        {
            if (cellComponent.Index + 1 >= totalCount)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.ColumnPos + 1 != cellColumnPos[cellComponent.Index + 1])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index + 1;
                }
            }
        }

        // Top left neighbor = 5
        {
            if (cellComponent.Index + columns - 1 >= totalCount)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.ColumnPos - 1 != cellColumnPos[cellComponent.Index + columns - 1] &&
                    cellComponent.RowPos + 1 != cellRowPos[cellComponent.Index + columns - 1])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index + columns - 1;
                }
            }
        }

        // Top neighbor = 6
        {
            if (cellComponent.Index + columns >= totalCount)
            {
                tempIndex[countBuffers++] = -1;
            }
            else
            {
                if (cellComponent.RowPos + 1 != cellRowPos[cellComponent.Index + columns])
                {
                    tempIndex[countBuffers++] = -1;
                }
                else
                {
                    tempIndex[countBuffers++] = cellComponent.Index + columns;
                }
            }
        }

        // Top right neighbor = 7
        {
            if (cellComponent.Index + columns + 1 >= totalCount)
            {
                tempIndex[countBuffers] = -1;
            }
            else
            {
                if (cellComponent.ColumnPos != cellColumnPos[cellComponent.Index + columns + 1] &&
                    cellComponent.RowPos + 1 != cellRowPos[cellComponent.Index + columns + 1])
                {
                    tempIndex[countBuffers] = -1;
                }
                else
                {
                    tempIndex[countBuffers] = cellComponent.Index + columns + 1;
                }
            }
        }

        tempIndex.Sort();
        
        foreach (var neighborIndex in tempIndex)
        {
            neighborCells.Add(new NeighborCell
            {
                Index = neighborIndex,
            });
        }

        tempIndex.Dispose();
    }
}