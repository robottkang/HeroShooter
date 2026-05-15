using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI hostNameText;
    [SerializeField] private Button joinButton;

    public void Setup(string roomName, string hostName, Action onJoin)
    {
        roomNameText.text = roomName;
        hostNameText.text = hostName;
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onJoin?.Invoke());
    }
}
