using UnityEngine;

public class PlayerUISpawner : MonoBehaviour
{
    [SerializeField] private HUDManager hudManager;

    private void Awake()
    {
        Instantiate(hudManager);
    }
}
