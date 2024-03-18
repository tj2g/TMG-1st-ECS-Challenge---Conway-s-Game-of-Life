using Unity.Entities;
using Random = Unity.Mathematics.Random;
using UnityEngine;

public class ConfigAuthoring : MonoBehaviour
{
    public int Columns;
    public int Rows;
    public GameObject CellPrefab;

    private class Baker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new ConfigComponent
            {
                Columns = authoring.Columns,
                Rows = authoring.Rows,
                CellPrefab = GetEntity(authoring.CellPrefab, TransformUsageFlags.Dynamic)
            });
            AddComponent(entity, new CellRandom
            {
                Value = new Random((uint)(float)System.DateTime.Now.TimeOfDay.TotalMilliseconds)
            });
        }
    }
}
