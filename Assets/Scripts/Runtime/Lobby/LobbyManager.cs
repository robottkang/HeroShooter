using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using TMPro;
using Cysharp.Threading.Tasks;

public class LobbyManager : NetworkBehaviour, INetworkRunnerCallbacks, IPlayerLeft, IStateAuthorityChanged
{
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Panel")]
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private BrowseController browsePanel;
    [SerializeField] private SessionManagementController sessionManagementPanel;
    [SerializeField] private PlayerReadyController playerReadyPanel;

    [Header("Prefab")]
    [SerializeField] private NetworkRunner runnerPrefab;

    [Networked, Capacity(2)]
    private NetworkArray<LobbyPlayerData> Players { get => default; }

    [Networked, OnChangedRender(nameof(NotifyStateChanged))]
    private NetworkString<_16> SelectedMap { get => default; set { } }

    [Networked, OnChangedRender(nameof(NotifyStateChanged))]
    private int DirtyFlag { get => default; set { } }

    private NetworkRunner _lobbyRunner;
    private string _pendingMap;
    private bool _isMatchStarting;

    private void Start()
    {
        browsePanel.OnJoinSessionRequested += sessionName => JoinSession(sessionName).Forget();

        sessionManagementPanel.OnCreateSessionRequested += (sessionName, mapName) => CreateSession(sessionName, mapName).Forget();
        //sessionManagementPanel.OnCreateSessionRequested += (_, _) => NotifyStateChanged();
        sessionManagementPanel.OnBackRequested += ShowBrowsePanel;

        playerReadyPanel.OnReadyClicked += RequestToggleReady;

        ShowBrowsePanel();
        connectingPanel.SetActive(true);
        ConnectToLobby().Forget();
    }

    public override void Spawned()
    {
        Debug.Log("Spawned");
        if (Object.HasStateAuthority)
        {
            SelectedMap = "Map1";
            AddPlayer(Runner.LocalPlayer, LocalPlayerData.NickName);
        }
        else
        {
            RPC_RequestJoin(LocalPlayerData.NickName);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _lobbyRunner.RemoveCallbacks(this);
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
            RemovePlayer(player);
    }

    public void StateAuthorityChanged() => NotifyStateChanged();

    public void RequestToggleReady() => RPC_ToggleReady(Runner.LocalPlayer);

    public void RequestSetMap(string mapName)
    {
        if (!Object.HasStateAuthority) return;
        SelectedMap = mapName;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestJoin(NetworkString<_32> playerName, RpcInfo info = default)
    {
        AddPlayer(info.Source, playerName.ToString());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ToggleReady(PlayerRef playerRef)
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].PlayerRef != playerRef) continue;

            var slot = Players.Get(i);
            slot.IsReady = !slot.IsReady;
            Players.Set(i, slot);
            BumpDirty();
            TryStartMatch();
            return;
        }
    }

    private void AddPlayer(PlayerRef playerRef, string name)
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (!Players[i].IsEmpty) continue;

            Players.Set(i, new LobbyPlayerData
            {
                PlayerRef = playerRef,
                PlayerName = name,
                IsReady = false
            });
            BumpDirty();
            return;
        }
    }

    private void RemovePlayer(PlayerRef playerRef)
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].PlayerRef != playerRef) continue;

            Players.Set(i, default);
            BumpDirty();
            return;
        }
    }

    private void BumpDirty()
    {
        if (Object.HasStateAuthority) DirtyFlag++;
    }

    private void NotifyStateChanged()
    {
        ShowPlayerReadyPanel();

        if (Object.HasStateAuthority &&
            !string.IsNullOrEmpty(_pendingMap))
        {
            RequestSetMap(_pendingMap);
            _pendingMap = null;
        }

        playerReadyPanel.Refresh(Players, Runner.LocalPlayer);
    }

    private void ShowBrowsePanel()
    {
        browsePanel.Show();
        playerReadyPanel.Hide();
    }

    private void ShowPlayerReadyPanel()
    {
        browsePanel.Hide();
        playerReadyPanel.Show();
    }

    private async UniTask ConnectToLobby()
    {
        _lobbyRunner = FindFirstObjectByType<NetworkRunner>();
        if (_lobbyRunner == null)
            _lobbyRunner = Instantiate(runnerPrefab);

        _lobbyRunner.AddCallbacks(this);
        var result = await _lobbyRunner.JoinSessionLobby(SessionLobby.Shared);
        if (result.Ok)
            connectingPanel.SetActive(false);
        else
            Debug.LogError($"[Lobby] JoinSessionLobby failed: {result.ShutdownReason}");
    }

    private async UniTask CreateSession(string sessionName, string mapName)
    {
        LocalPlayerData.NickName = playerNameInput.text.Trim();
        _pendingMap = mapName;

        var props = new Dictionary<string, SessionProperty>
        {
            { SessionKeys.HostName, LocalPlayerData.NickName },
            { SessionKeys.CreatedTime, DateTime.Now.ToString("HHmmssfff") }
        };

        var result = await _lobbyRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            SessionProperties = props,
            PlayerCount = 2,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
        });

        if (result.Ok)
            ShowPlayerReadyPanel();
        else
            Debug.LogError($"[Lobby] CreateSession failed: {result.ShutdownReason}");
    }

    private async UniTask JoinSession(string sessionName)
    {
        LocalPlayerData.NickName = playerNameInput.text.Trim();

        var result = await _lobbyRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
        });

        if (result.Ok)
            ShowPlayerReadyPanel();
        else
            Debug.LogError($"[Lobby] CreateSession failed: {result.ShutdownReason}");
    }

    private void TryStartMatch()
    {
        if (!Object.HasStateAuthority || _isMatchStarting) return;

        foreach (var p in Players)
            if (p.IsEmpty || !p.IsReady) return;

        if (Object.HasStateAuthority)
        {
            _lobbyRunner.SessionInfo.IsVisible = false;
            _lobbyRunner.SessionInfo.IsOpen = false;
        }

        _isMatchStarting = true;
        int buildIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{SelectedMap}.unity");
        Runner.LoadScene(SceneRef.FromIndex(buildIndex));
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        browsePanel.UpdateSessions(sessionList);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        ShowBrowsePanel();
        connectingPanel.SetActive(true);
        Destroy(_lobbyRunner.gameObject);
        _lobbyRunner = null;
        ConnectToLobby().Forget();
    }

    public void OnStartGameFailed(NetworkRunner runner, ShutdownReason reason)
    {
        Debug.LogError($"[Lobby] StartGame failed: {reason}");
    }

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
