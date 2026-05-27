using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Cysharp.Threading.Tasks;

public class GameManager : SimulationBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private GameObject localPlayerArmsPrefab;
    [SerializeField] private Transform[] spawnPoints;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool isDebug;
#else
    private bool isDebug = false;
#endif


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SpawnPlayer();
        Camera.main.gameObject.SetActive(false);
    }

    private void SpawnPlayer()
    {
        string sessionName = Runner.SessionInfo.Name;
        string sessionCreatedTime = Runner.SessionInfo.Properties.TryGetValue(SessionKeys.CreatedTime, out var ct) ?
            ct.PropertyValue.ToString() : "";
        int seed = (sessionName + sessionCreatedTime).GetHashCode();

        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (Runner.IsSharedModeMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];
        
        //Instantiate(localPlayerArmsPrefab, spawnPoint.position, spawnPoint.rotation);
        Runner.Spawn(playerPrefab, spawnPoint.position, spawnPoint.rotation, Runner.LocalPlayer);
    }
}
