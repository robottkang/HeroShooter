using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            Debug.Log(PhotonNetwork.CurrentRoom.ToString());
        else
            Debug.LogError("Is not connected");

        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(5f);

        int seed = PhotonNetwork.CurrentRoom.Name.GetHashCode() + PhotonNetwork.MasterClient.NickName.GetHashCode();
        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (PhotonNetwork.IsMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public override void OnJoinedRoom()
    {
        int seed = PhotonNetwork.CurrentRoom.Name.GetHashCode() + PhotonNetwork.MasterClient.NickName.GetHashCode();
        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (PhotonNetwork.IsMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];

        //PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"new player: {newPlayer.NickName}");
    }
}
