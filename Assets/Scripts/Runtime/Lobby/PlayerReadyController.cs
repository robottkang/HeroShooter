using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using UnityEngine.Events;

public class PlayerReadyController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI player1InfoText;
    [SerializeField] private TextMeshProUGUI player2InfoText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Color unreadyColor;
    [SerializeField] private Color readyColor;

    public event Action OnReadyClicked;

    public bool IsVisible => gameObject.activeSelf;

    private void Start()
    {
        readyButton.onClick.AddListener(() => OnReadyClicked?.Invoke());
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Refresh(NetworkArray<LobbyPlayerData> players, PlayerRef localPlayer)
    {
        UpdateSlot(player1InfoText, players[0], localPlayer);
        UpdateSlot(player2InfoText, players[1], localPlayer);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].PlayerRef != localPlayer) continue;
            var colors = readyButton.colors;
            colors.normalColor = players[i].IsReady ? readyColor : unreadyColor;
            readyButton.colors = colors;
            break;
        }
    }

    private void UpdateSlot(TextMeshProUGUI label, LobbyPlayerData data, PlayerRef localPlayer)
    {
        if (data.IsEmpty)
        {
            label.text = "대기 중...";
            return;
        }

        string readyMark = data.IsReady ? " [READY]" : string.Empty;
        string meMark = data.PlayerRef == localPlayer ? " (나)" : string.Empty;
        label.text = $"{data.PlayerName}{readyMark}{meMark}";
    }
}
