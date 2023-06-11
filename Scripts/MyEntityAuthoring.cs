using Unity.Entities;
using UnityEngine;

public class MyEntityAuthoring : MonoBehaviour
{
   
}

class MyEntityBaker : Baker<MyEntityAuthoring>
{
    public override void Bake(MyEntityAuthoring authoring)
    {
        AddComponent(new MyEntity
        {
            position = authoring.transform.position
        });
    }
}