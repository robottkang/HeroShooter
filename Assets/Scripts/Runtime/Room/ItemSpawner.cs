using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : SimulationBehaviour
{
    [SerializeField] private ItemBase[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        RegisterOnRunner();
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        var prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        Runner.SpawnAsync(prefab.gameObject, spawnPoint.position, spawnPoint.rotation);
    }

    private void RegisterOnRunner()
    {
        if (NetworkRunner.Instances.Count == 0) return;

        var runner = NetworkRunner.Instances[0];
        if (runner != null && runner.IsRunning)
            runner.AddGlobal(this);
    }
}
