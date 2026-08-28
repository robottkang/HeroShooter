using Fusion;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private ItemBase[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;

    public override void Spawned()
    {
        string sessionCreatedTime = Runner.SessionInfo.Properties.TryGetValue(SessionKeys.CreatedTime, out var ct) ?
            ct.PropertyValue.ToString() : "";
        var rng = new System.Random(sessionCreatedTime.GetHashCode());
        var prefab = itemPrefabs[rng.Next(itemPrefabs.Length)];
        Runner.SpawnAsync(prefab.gameObject, spawnPoint.position, spawnPoint.rotation);
    }
}
