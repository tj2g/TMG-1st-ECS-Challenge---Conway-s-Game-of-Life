using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(SpawnSystem))]
public partial struct CameraMovementSystem : ISystem
{
    private int i;

    public void OnCreate()
    {
        // Quick ghetto fix to make sure the camera doesn't reorient to original point on movement
        i = 0;
    }

    public void OnUpdate(ref SystemState state)
    {
        // Setting up camera for going to center of grid
        var cellConfig = SystemAPI.GetSingleton<ConfigComponent>();
        int columns = cellConfig.Columns;
        int rows = cellConfig.Rows;

        var movement = new float3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetAxis("Mouse ScrollWheel") * 50);

        // For Moving the camera with player controls
        movement *= SystemAPI.Time.DeltaTime * 100.0f;

        var cameraTransform = Camera.main.transform;

        if (i == 0)
        {
            cameraTransform.position = new Vector3(columns / 2, rows / 2, -40);
            i++;
        }

        cameraTransform.position += new Vector3(movement.x, movement.y, movement.z);

        if (cameraTransform.position.z >= -10)
            cameraTransform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, -10);
        //if (cameraTransform.position.z <= -100)
        //    cameraTransform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, -100);

    }
}