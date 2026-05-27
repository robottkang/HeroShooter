using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private BrowseController browsePanel;
    [SerializeField] private RoomManagementController createRoomPanel;
    [SerializeField] private SessionController sessionPanel;
    [SerializeField] private NetworkRunner runnerPrefab;

    private NetworkRunner _runner;
    private string _pendingMap;

    private void OnEnable() => LobbySessionController.OnStateChanged += OnSessionStateChanged;
    private void OnDisable() => LobbySessionController.OnStateChanged -= OnSessionStateChanged;

    private void Start()
    {
        browsePanel.OnJoinRoomRequested += JoinRoom;

        createRoomPanel.OnCreateRoomRequested += CreateRoom;
        createRoomPanel.OnBackRequested += ShowBrowsePanel;

        sessionPanel.OnReadyClicked += () => LobbySessionController.Instance?.RequestToggleReady();
        sessionPanel.OnMapSelected += mapName => LobbySessionController.Instance?.RequestSetMap(mapName);

        ShowBrowsePanel();
        connectingPanel.SetActive(true);
        ConnectToLobby();
    }

    private void ShowBrowsePanel()
    {
        browsePanel.Show();
        createRoomPanel.Hide();
        sessionPanel.Hide();
    }

    private void ShowCreateRoomPanel()
    {
        browsePanel.Hide();
        createRoomPanel.Show();
    }

    private void ShowSessionPanel()
    {
        browsePanel.Hide();
        createRoomPanel.Hide();
        sessionPanel.Show(_runner.SessionInfo.Name);
    }

    private async void ConnectToLobby()
    {
        _runner = FindFirstObjectByType<NetworkRunner>();
        if (_runner == null )
            _runner = Instantiate(runnerPrefab);

        _runner.AddCallbacks(this);
        var result = await _runner.JoinSessionLobby(SessionLobby.Shared);
        if (result.Ok)
            connectingPanel.SetActive(false);
        else
            Debug.LogError($"[Lobby] JoinSessionLobby failed: {result.ShutdownReason}");
    }

    private async void CreateRoom(string roomName, string playerName, string mapName)
    {
        LocalPlayerData.PlayerName = playerName;
        _pendingMap = mapName;

        var props = new Dictionary<string, SessionProperty>
        {
            { SessionKeys.HostName, playerName },
            { SessionKeys.CreatedTime, DateTime.Now.ToString("HHmmssfff") }
        };

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            SessionProperties = props,
            PlayerCount = 2,
        });

        if (!result.Ok)
        {
            Debug.LogError($"[Lobby] CreateRoom failed: {result.ShutdownReason}");
            createRoomPanel.SetConfirmInteractable(true);
        }
    }

    private async void JoinRoom(string roomName)
    {
        LocalPlayerData.PlayerName = browsePanel.PlayerName;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
        });

        if (!result.Ok)
            Debug.LogError($"[Lobby] JoinRoom failed: {result.ShutdownReason}");
    }

    private void OnSessionStateChanged()
    {
        var ctrl = LobbySessionController.Instance;
        if (ctrl == null) return;

        if (!sessionPanel.IsVisible)
        {
            ShowSessionPanel();

            if (ctrl.Object.HasStateAuthority && !string.IsNullOrEmpty(_pendingMap))
            {
                ctrl.RequestSetMap(_pendingMap);
                _pendingMap = null;
            }
        }

        sessionPanel.Refresh(ctrl.Players, ctrl.SelectedMap.ToString(), _runner.LocalPlayer, ctrl.Object.HasStateAuthority);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        => browsePanel.UpdateSessions(sessionList);

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        ShowBrowsePanel();
        connectingPanel.SetActive(true);
        Destroy(_runner.gameObject);
        _runner = null;
        ConnectToLobby();
    }

    public void OnStartGameFailed(NetworkRunner runner, ShutdownReason reason)
        => Debug.LogError($"[Lobby] StartGame failed: {reason}");

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
