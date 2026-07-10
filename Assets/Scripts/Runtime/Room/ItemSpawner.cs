using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private ItemBase[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;

    public void Awake()
    {
        if (Runner != null)
            RegisterOnRunner();
    }

    public void Start()
    {
        Debug.Log($"{Runner}, {(Runner ? Runner.IsServer : "Nope")}");
        if (Runner == null || Runner.IsServer) return;
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        var prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        Runner.Spawn(prefab.gameObject, spawnPoint.position, spawnPoint.rotation);
    }

    public void RegisterOnRunner()
    {
        var runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (runner.IsRunning)
            runner.AddGlobal(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
}
