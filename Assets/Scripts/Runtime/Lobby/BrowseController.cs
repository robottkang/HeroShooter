using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class BrowseController : MonoBehaviour
{
    [SerializeField] private int defaultSessionCount = 5;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject sessionListItemPrefab;

    private List<SessionInfo> _sessions = new();
    private readonly List<GameObject> _sessionItems = new();

    public event Action<string> OnJoinRoomRequested;

    public string PlayerName
    {
        get
        {
            string name = playerNameInput.text.Trim();
            return string.IsNullOrEmpty(name) ? "Player" : name;
        }
    }

    private void Start()
    {
        refreshButton.onClick.AddListener(() => RedrawSessionList(defaultSessionCount));
        searchInput.onValueChanged.AddListener(_ => RedrawSessionList(defaultSessionCount));
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateSessions(List<SessionInfo> sessions)
    {
        _sessions = sessions;
    }

    public void RedrawSessionList(int redrawSessionCount)
    {
        foreach (var item in _sessionItems)
            Destroy(item);
        _sessionItems.Clear();

        string search = searchInput != null ? searchInput.text.Trim().ToLower() : string.Empty;
        bool hasSearch = !string.IsNullOrEmpty(search);

        int count = 0;
        foreach (var room in _sessions)
        {
            if (!room.IsOpen || !room.IsVisible) continue;
            if (hasSearch && !room.Name.ToLower().Contains(search)) continue;
            if (!hasSearch && count >= redrawSessionCount) break;

            string hostName = room.Properties.TryGetValue(SessionKeys.HostName, out var hn)
                ? hn.ToString() : "Unknown";

            string capturedName = room.Name;
            var go = Instantiate(sessionListItemPrefab, roomListContent);
            go.GetComponent<RoomListItem>().Setup(capturedName, hostName, () => OnJoinRoomRequested?.Invoke(capturedName));
            _sessionItems.Add(go);
            count++;
        }
    }
}
