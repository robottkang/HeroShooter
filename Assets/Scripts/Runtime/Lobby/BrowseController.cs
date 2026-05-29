using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using UnityEngine.Events;

public class BrowseController : MonoBehaviour
{
    [SerializeField] private int defaultSessionCount = 5;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Transform sessionListContent;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject sessionListItemPrefab;

    private List<SessionInfo> _sessions = new();
    private readonly List<GameObject> _sessionItems = new();

    public delegate void OnJoinSessionHandler(string sessionName);
    public event OnJoinSessionHandler OnJoinSessionRequested;

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
        foreach (var session in _sessions)
        {
            if (!session.IsOpen || !session.IsVisible) continue;
            if (hasSearch && !session.Name.ToLower().Contains(search)) continue;
            if (!hasSearch && count >= redrawSessionCount) break;

            string hostName = session.Properties.TryGetValue(SessionKeys.HostName, out var hn)
                ? hn.PropertyValue.ToString() : "Unknown";

            string capturedName = session.Name;
            var go = Instantiate(sessionListItemPrefab, sessionListContent);
            go.GetComponent<RoomListItem>()
                .Setup(capturedName, hostName, () => OnJoinSessionRequested?.Invoke(capturedName));
            _sessionItems.Add(go);
            count++;
        }
    }
}
