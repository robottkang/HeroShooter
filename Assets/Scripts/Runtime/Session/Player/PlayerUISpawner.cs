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
        //var playerController = GetComponent<PlayerController>();

        //hudInstance.Init(playerController);
    }
}
