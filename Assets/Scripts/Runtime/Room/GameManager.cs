using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject localPlayerArmsPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Button startBtn;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool isDebug;
#else
    private bool isDebug = false;
#endif


    private void Start()
    {
        startBtn.gameObject.SetActive(false);

        startBtn.onClick.AddListener(() =>
        {
            SpawnPlayer();
            startBtn.gameObject.SetActive(false);
            Camera.main.gameObject.SetActive(false);
        });

        ControlStartButton().Forget();
    }

    private async UniTask ControlStartButton()
    {
        try
        {
            await UniTask.WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount >= 2);

            startBtn.gameObject.SetActive(true);
        }
        catch when (isDebug)
        {
            Instantiate(localPlayerArmsPrefab, spawnPoints[0].position, spawnPoints[0].rotation);
        }
        catch (System.Exception e)
        {
            throw e;
        }
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
