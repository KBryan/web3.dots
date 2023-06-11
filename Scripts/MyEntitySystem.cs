using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct MyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<MyEntity> myEntity in SystemAPI.Query<RefRW<MyEntity>>())
        {
            Debug.Log("My Entity Position: " + myEntity.ValueRO.position);
            Vector3 newPosition = new Vector3(0, 0, 0);
            myEntity.ValueRW.position = newPosition;
            Debug.Log("My Entity Position: " + myEntity.ValueRO.position);

        }
    }
}
