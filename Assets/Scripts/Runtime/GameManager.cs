using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        int seed = PhotonNetwork.CurrentRoom.Name.GetHashCode() + PhotonNetwork.MasterClient.NickName.GetHashCode();
        var rng = new System.Random(seed);

        Transform spawnPoint;
        if (PhotonNetwork.IsMasterClient ^ (rng.Next(0, 2) == 0))
            spawnPoint = spawnPoints[0];
        else
            spawnPoint = spawnPoints[1];

        PhotonNetwork.Instantiate("Player", spawnPoint.position, spawnPoint.rotation);
    }
}
