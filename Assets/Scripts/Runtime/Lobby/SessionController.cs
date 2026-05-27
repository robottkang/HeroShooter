using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class SessionController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapPreviewImage;
    [SerializeField] private TextMeshProUGUI player1InfoText;
    [SerializeField] private TextMeshProUGUI player2InfoText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Color unreadyColor;
    [SerializeField] private Color readyColor;

    [Header("Map Config")]
    [SerializeField] private string[] maps = { "Map1", "Map2" };
    [SerializeField] private Sprite[] mapPreviewSprites;
    [SerializeField] private Sprite randomPreviewSprite;

    public event Action OnReadyClicked;
    public event Action<string> OnMapSelected;

    public bool IsVisible => gameObject.activeSelf;

    private void Start()
    {
        readyButton.onClick.AddListener(() => OnReadyClicked?.Invoke());
        mapDropdown.onValueChanged.AddListener(OnDropdownChanged);

        PopulateMapDropdown();
    }

    public void Show(string roomName)
    {
        gameObject.SetActive(true);
        roomNameText.text = roomName;
    }

    public void Hide() => gameObject.SetActive(false);

    public void Refresh(NetworkArray<LobbyPlayerData> players, string selectedMap, PlayerRef localPlayer, bool isHost)
    {
        UpdateSlot(player1InfoText, players[0], localPlayer);
        UpdateSlot(player2InfoText, players[1], localPlayer);

        mapDropdown.interactable = isHost;

        int idx = Array.IndexOf(maps, selectedMap);
        int dropdownValue = idx >= 0 ? idx + 1 : 0;
        mapPreviewImage.sprite = GetMapSprite(dropdownValue);
        if (!isHost)
            mapDropdown.SetValueWithoutNotify(dropdownValue);

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

    private void OnDropdownChanged(int index)
    {
        mapPreviewImage.sprite = GetMapSprite(index);
        OnMapSelected?.Invoke(ResolveMap(index));
    }

    private void PopulateMapDropdown()
    {
        var options = new System.Collections.Generic.List<string> { "Random" };
        foreach (var map in maps) options.Add(map);
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(options);
        mapDropdown.SetValueWithoutNotify(0);
    }

    private string ResolveMap(int dropdownIndex)
    {
        if (maps.Length == 0) return "Map1";
        if (dropdownIndex == 0) return maps[UnityEngine.Random.Range(0, maps.Length)];
        return maps[Mathf.Clamp(dropdownIndex - 1, 0, maps.Length - 1)];
    }

    private Sprite GetMapSprite(int dropdownIndex)
    {
        if (dropdownIndex == 0) return randomPreviewSprite;
        int idx = dropdownIndex - 1;
        return idx < mapPreviewSprites.Length ? mapPreviewSprites[idx] : null;
    }
}
