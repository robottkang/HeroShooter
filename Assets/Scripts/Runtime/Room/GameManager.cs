using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Button startBtn;

    private bool _spawned;

    private void Start()
    {
        startBtn.gameObject.SetActive(false);

        startBtn.onClick.AddListener(() =>
        {
            SpawnPlayer();
            startBtn.gameObject.SetActive(false);
            Camera.main.gameObject.SetActive(false);
        });

        StartCoroutine(ControlStartButton());
    }

    private IEnumerator ControlStartButton()
    {
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount >= 2);

        startBtn.gameObject.SetActive(true);
    }

    private void SpawnPlayer()
    {
        _spawned = true;

        int seed = PhotonNetwork.CurrentRoom.Name.GetHashCode() + PhotonNetwork.MasterClient.NickName.GetHashCode();
        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (PhotonNetwork.IsMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }
}
