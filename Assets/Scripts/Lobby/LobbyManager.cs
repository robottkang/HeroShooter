using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] Button createRoomButton;
    [SerializeField] TMP_InputField searchInput;
    [SerializeField] Transform roomListContent;
    [SerializeField] Button refreshButton;
    [SerializeField] GameObject connectingPanel;

    [Header("Game Scenes")]
    [SerializeField] string[] gameScenes = { "Stage1" };

    [Header("Map Selection")]
    [SerializeField] TMP_Dropdown mapDropdown;
    [SerializeField] Image mapPreviewImage;
    [SerializeField] Sprite[] mapPreviewSprites;
    [SerializeField] Sprite randomPreviewSprite;

    [Header("Prefabs")]
    [SerializeField] GameObject roomListItemPrefab;

    const string HostNameKey = "hn";
    const int DefaultRoomDisplayLimit = 4;

    readonly List<RoomInfo> _cachedRooms = new();
    readonly List<GameObject> _roomItems = new();

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        refreshButton.onClick.AddListener(OnRefreshClicked);
        searchInput.onValueChanged.AddListener(_ => RedrawList());
        PopulateMapDropdown();
        mapDropdown.onValueChanged.AddListener(OnMapSelected);

        connectingPanel.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    void PopulateMapDropdown()
    {
        mapDropdown.ClearOptions();
        var options = new List<string> { "Random" };
        options.AddRange(gameScenes);
        mapDropdown.AddOptions(options);
        OnMapSelected(0);
    }

    void OnMapSelected(int index)
    {
        if (mapPreviewImage == null) return;
        int sceneIndex = index - 1;
        Sprite sprite = index == 0 ? randomPreviewSprite
            : (mapPreviewSprites != null && sceneIndex < mapPreviewSprites.Length)
                ? mapPreviewSprites[sceneIndex] : null;
        mapPreviewImage.sprite = sprite;
        mapPreviewImage.color  = sprite != null ? Color.white : new Color(0.1f, 0.1f, 0.1f);
    }

    string GetSelectedScene()
    {
        int index = mapDropdown != null ? mapDropdown.value : 0;
        if (index == 0)
            return gameScenes[Random.Range(0, gameScenes.Length)];
        return gameScenes[index - 1];
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        connectingPanel.SetActive(false);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var room in roomList)
        {
            if (room.RemovedFromList)
            {
                _cachedRooms.RemoveAll(r => r.Name == room.Name);
            }
            else
            {
                int idx = _cachedRooms.FindIndex(r => r.Name == room.Name);
                if (idx >= 0) _cachedRooms[idx] = room;
                else _cachedRooms.Add(room);
            }
        }
        RedrawList();
    }

    void OnCreateRoomClicked()
    {
        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName)) return;

        string playerName = playerNameInput.text.Trim();
        PhotonNetwork.NickName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;

        var options = new RoomOptions
        {
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { HostNameKey, PhotonNetwork.NickName }
            },
            CustomRoomPropertiesForLobby = new[] { HostNameKey },
            MaxPlayers = 8
        };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    void OnRefreshClicked() => RedrawList();

    void RedrawList()
    {
        foreach (var item in _roomItems) Destroy(item);
        _roomItems.Clear();

        string search = searchInput != null ? searchInput.text.Trim().ToLower() : string.Empty;
        bool hasSearch = !string.IsNullOrEmpty(search);

        int count = 0;
        foreach (var room in _cachedRooms)
        {
            if (!room.IsOpen || !room.IsVisible) continue;
            if (hasSearch && !room.Name.ToLower().Contains(search)) continue;
            if (!hasSearch && count >= DefaultRoomDisplayLimit) break;

            string hostName = room.CustomProperties.TryGetValue(HostNameKey, out var hn)
                ? hn.ToString() : "Unknown";

            string capturedName = room.Name;
            var go = Instantiate(roomListItemPrefab, roomListContent);
            go.GetComponent<RoomListItem>().Setup(capturedName, hostName, () => JoinRoom(capturedName));
            _roomItems.Add(go);
            count++;
        }
    }

    void JoinRoom(string roomName)
    {
        string playerName = playerNameInput.text.Trim();
        PhotonNetwork.NickName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.LoadLevel(GetSelectedScene());
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Lobby] CreateRoom failed ({returnCode}): {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Lobby] JoinRoom failed ({returnCode}): {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.None || cause == DisconnectCause.DisconnectByClientLogic) return;
        if (this == null) return;
        connectingPanel.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }
}
