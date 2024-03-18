using System;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

public struct ConfigComponent : IComponentData
{
    public int Columns;
    public int Rows;
    public Entity CellPrefab;
}

public struct CellRandom : IComponentData
{
    public Random Value;
}