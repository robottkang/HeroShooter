using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject connectingPanel;

    [Header("Map Selection")]
    [SerializeField] private string[] maps = { };
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapPreviewImage;
    [SerializeField] private Sprite[] mapPreviewSprites;
    [SerializeField] private Sprite randomPreviewSprite;

    [Header("Prefabs")]
    [SerializeField] private GameObject roomListItemPrefab;

    private const string HostNameKey = "hn";
    private const int DefaultRoomDisplayLimit = 4;

    private readonly List<RoomInfo> _cachedRooms = new();
    private readonly List<GameObject> _roomItems = new();

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        refreshButton.onClick.AddListener(OnRefreshClicked);
        searchInput.onValueChanged.AddListener(_ => RedrawList());
        PopulateMapDropdown();
        mapDropdown.onValueChanged.AddListener(OnMapSelected);

        connectingPanel.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    private void PopulateMapDropdown()
    {
        mapDropdown.ClearOptions();
        var options = new List<string> { "Random" };
        options.AddRange(maps);
        mapDropdown.AddOptions(new List<string>(options));
        OnMapSelected(0);
    }

    private void OnMapSelected(int index)
    {
        if (mapPreviewImage == null) return;

        int sceneIndex = index - 1;
        Sprite sprite = index == 0 ? randomPreviewSprite
            : (mapPreviewSprites != null && sceneIndex < mapPreviewSprites.Length)
                ? mapPreviewSprites[sceneIndex] : null;
        mapPreviewImage.sprite = sprite;
    }

    private string GetSelectedScene()
    {
        if (mapDropdown.value == 0)
            return maps[Random.Range(0, maps.Length)];
        return maps[mapDropdown.value];
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
    }

    private void OnCreateRoomClicked()
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
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    private void OnRefreshClicked() => RedrawList();

    private void RedrawList()
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

    private void JoinRoom(string roomName)
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
