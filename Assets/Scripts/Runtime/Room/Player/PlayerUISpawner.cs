using UnityEngine;

public class PlayerUISpawner : MonoBehaviour
{
    [SerializeField] private GameObject hudManagerPrefab;

    private HUDManager hudInstance;

    private void Awake()
    {
        hudInstance = Instantiate(hudManagerPrefab).GetComponent<HUDManager>();
    }

    private void Start()
    {
        var fpc = GetComponent<FirstPersonController>();
        var weaponInventory = GetComponentInChildren<WeaponInventory>();
        var health = GetComponent<Health>();

        hudInstance.Init(fpc, weaponInventory, health);
    }
}
