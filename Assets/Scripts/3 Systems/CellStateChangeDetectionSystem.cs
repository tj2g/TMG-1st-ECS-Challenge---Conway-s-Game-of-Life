using Unity.Entities;
using Unity.Burst;
using Unity.Collections;


[UpdateAfter(typeof(SettingNeighborsSystem))]
[BurstCompile]
public partial struct CellStateChangeDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<ConfigComponent>();
        int totalCount = config.Columns * config.Rows;
        var cellIndex = CollectionHelper.CreateNativeArray<int>(totalCount, state.WorldUpdateAllocator);
        var cellIsAlive = CollectionHelper.CreateNativeArray<bool>(totalCount, state.WorldUpdateAllocator);

        var getCurrentIndex = new GetCurrentIndex
        {
            cellIndex = cellIndex,
        }.ScheduleParallel(state.Dependency);

        getCurrentIndex.Complete();

        var getCurrentIsAlive = new GetCurrentIsAlive
        {
            cellIsAlive = cellIsAlive,
        }.ScheduleParallel(state.Dependency);

        getCurrentIsAlive.Complete();

        var setCellStatus = new SetCellStatus
        {
            cellIndex = cellIndex.AsReadOnly(),
            cellIsAlive = cellIsAlive.AsReadOnly(),
        }.ScheduleParallel(state.Dependency);

        setCellStatus.Complete();

    }
}

[BurstCompile]
public partial struct GetCurrentIndex : IJobEntity
{
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<int> cellIndex;

    public void Execute(in CellComponent cellComponent)
    {
        cellIndex[cellComponent.Index] = cellComponent.Index;
    }
}

[BurstCompile]
public partial struct GetCurrentColumnPos : IJobEntity
{
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<int> cellColumnPos;

    public void Execute(in CellComponent cellComponent)
    {
        cellColumnPos[cellComponent.Index] = cellComponent.ColumnPos;
    }
}

public partial struct GetCurrentRowPos : IJobEntity
{
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<int> cellRowPos;

    public void Execute(in CellComponent cellComponent)
    {
        cellRowPos[cellComponent.Index] = cellComponent.RowPos;
    }
}

[BurstCompile]
public partial struct GetCurrentIsAlive : IJobEntity
{
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<bool> cellIsAlive;

    public void Execute(in CellComponent cellComponent)
    {
        cellIsAlive[cellComponent.Index] = cellComponent.IsAlive;
    }
}

[BurstCompile]
public partial struct SetCellStatus : IJobEntity
{
    [NativeDisableParallelForRestriction]
    [ReadOnly] public NativeArray<int>.ReadOnly cellIndex;
    [ReadOnly] public NativeArray<bool>.ReadOnly cellIsAlive;

    public void Execute(ref CellComponent cellComponent, in DynamicBuffer<NeighborCell> cellNeighbors)
    {
        int activeNeighborCells = GetActiveCellsCount(cellNeighbors);

        if (activeNeighborCells <= 1 && cellComponent.IsAlive)
        {
            cellComponent.IsAlive = false;
        }
        else if (activeNeighborCells >= 4 && cellComponent.IsAlive)
        {
            cellComponent.IsAlive = false;
        }
        else if (activeNeighborCells == 3 && !cellComponent.IsAlive)
        {
            cellComponent.IsAlive = true;
        }
        else if (activeNeighborCells >= 2 && cellComponent.IsAlive)
        {
            cellComponent.IsAlive = true;
        }
    }
    private int GetActiveCellsCount(in DynamicBuffer<NeighborCell> cellNeighbors)
    {
        int activeCount = 0;
        for (int i = 0; i < cellNeighbors.Length; i++)
        {
            var cellIndex = cellNeighbors[i].Index;
            if (cellIndex == -1)
            {
                activeCount += 0;
            }
            else
            {
                if (cellIsAlive[cellIndex])
                {
                    activeCount += 1;
                }
                else
                {
                    activeCount += 0;
                }
            }
        }
        return activeCount;
    }
}
