using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private ItemBase[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        var prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
