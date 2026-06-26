using Fusion;
using UnityEngine;

public class ItemSpawner : SimulationBehaviour
{
    [SerializeField] private ItemBase[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;

    public void Awake()
    {
        if (Runner != null)
            RegisterOnRunner();
    }

    public void Start()
    {
        if (Runner == null || Runner.IsSharedModeMasterClient) return;
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        var prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        Runner.Spawn(prefab.gameObject, spawnPoint.position, spawnPoint.rotation);
    }

    public void RegisterOnRunner()
    {
        var runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (runner.IsRunning)
            runner.AddGlobal(this);
    }
}
