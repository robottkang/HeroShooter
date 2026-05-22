using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
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
        int seed = PhotonNetwork.CurrentRoom.Name.GetHashCode() + PhotonNetwork.MasterClient.NickName.GetHashCode();
        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (PhotonNetwork.IsMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];
        
        //Instantiate(localPlayerArmsPrefab, spawnPoint.position, spawnPoint.rotation);
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }
}
