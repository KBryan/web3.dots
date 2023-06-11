using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Entities;
using UnityEngine;
using Web3Unity.Scripts.Library.Ethers.Providers;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public float SpawnRate;

    private void Start()
    {
    }
 
    public async Task<string> GetAccount()
    {
        var proivder = new JsonRpcProvider("RPC_URL");
        var balance = await proivder.GetBlockNumber();
        Debug.Log("Balance: " + balance.Value);
        return balance.ToString();
    }
}

class SpawnBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        AddComponent(new Spawner
        {
            Prefab = GetEntity(authoring.Prefab),
            SpawnPosition = authoring.transform.position,
            NextSpawnTime = 0.0f,
            SpawnRate = authoring.SpawnRate
        });
    }
}