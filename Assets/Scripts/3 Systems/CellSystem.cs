using Unity.Entities;
using Unity.Rendering;
using Unity.Burst;
using Unity.Mathematics;


[UpdateAfter(typeof(SettingNeighborsSystem))]
[BurstCompile]
public partial struct CellSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new UpdatingColor().ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UpdatingColor : IJobEntity
{
    public void Execute(ref URPMaterialPropertyBaseColor color, in CellComponent cellComponent)
    {
        if (cellComponent.IsAlive)
        {
            color.Value = new float4(0, 1, 0, 1);
        }
        else
        {
            color.Value = new float4(0, 0, 0, 1);
        }
    }
}