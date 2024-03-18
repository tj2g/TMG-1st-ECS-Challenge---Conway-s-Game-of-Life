using Unity.Entities;

public struct CellComponent : IComponentData
{
    public int Index;
    public int ColumnPos;
    public int RowPos;
    public bool IsAlive;
}
