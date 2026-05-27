using System;
using Fusion;
using UnityEngine;

public class LobbySessionController : NetworkBehaviour, IPlayerLeft, IStateAuthorityChanged
{
    public static LobbySessionController Instance { get; private set; }
    public static event Action OnStateChanged;

    [Networked, Capacity(2)]
    public NetworkArray<LobbyPlayerData> Players { get; }

    [Networked, OnChangedRender(nameof(NotifyStateChanged))]
    public NetworkString<_16> SelectedMap { get; private set; }

    [Networked, OnChangedRender(nameof(NotifyStateChanged))]
    private int _dirtyFlag { get; set; }

    private bool _isMatchStarting;

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority)
        {
            SelectedMap = "Map1";
            AddPlayer(Runner.LocalPlayer, LocalPlayerData.PlayerName);
        }
        else
        {
            RPC_RequestJoin(LocalPlayerData.PlayerName);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this) Instance = null;
    }

    // IPlayerLeft: NetworkBehaviour 레벨에서 직접 처리 - runner 콜백 등록 불필요
    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
            RemovePlayer(player);
    }

    // IStateAuthorityChanged: 호스트 이탈 시 authority 이전 후 호출
    public void StateAuthorityChanged()
    {
        NotifyStateChanged();
    }

    public void RequestToggleReady()
    {
        RPC_ToggleReady(Runner.LocalPlayer);
    }

    public void RequestSetMap(string mapName)
    {
        if (!Object.HasStateAuthority) return;
        SelectedMap = mapName;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestJoin(NetworkString<_32> playerName, RpcInfo info = default)
        => AddPlayer(info.Source, playerName.ToString());

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

    private void TryStartMatch()
    {
        if (!Object.HasStateAuthority || _isMatchStarting) return;

        foreach (var p in Players)
            if (p.IsEmpty || !p.IsReady) return;

        _isMatchStarting = true;
        Runner.LoadScene(SelectedMap.ToString());
    }

    private void BumpDirty()
    {
        if (Object.HasStateAuthority) _dirtyFlag++;
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
