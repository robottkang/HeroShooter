using Fusion;

public struct LobbyPlayerData : INetworkStruct
{
    public PlayerRef PlayerRef;
    public NetworkBool IsReady;
    public NetworkString<_32> PlayerName;

    public bool IsEmpty => PlayerRef == PlayerRef.None;
}
