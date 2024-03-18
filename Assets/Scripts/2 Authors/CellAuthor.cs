using Unity.Entities;
using UnityEngine;

public class CellAuthor : MonoBehaviour
{
    public int Index;
    public int ColumnsPos;
    public int RowsPos;
    public bool IsAlive;

    private class Baker : Baker<CellAuthor>
    {
        public override void Bake(CellAuthor authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new CellComponent
            {
                Index = authoring.Index,
                ColumnPos = authoring.ColumnsPos,
                RowPos = authoring.RowsPos,
                IsAlive = authoring.IsAlive,
            });
            AddBuffer<NeighborCell>(entity);
        }
    }
}

[InternalBufferCapacity(8)]
public struct NeighborCell : IBufferElementData
{
    public int Index;
}