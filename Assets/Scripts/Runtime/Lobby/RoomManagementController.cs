using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomManagementController : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapPreviewImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Map Config")]
    [SerializeField] private string[] maps = { "Map1", "Map2" };
    [SerializeField] private Sprite[] mapPreviewSprites;
    [SerializeField] private Sprite randomPreviewSprite;

    public event Action<string, string, string> OnCreateRoomRequested;
    public event Action OnBackRequested;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(() => OnBackRequested?.Invoke());
        mapDropdown.onValueChanged.AddListener(OnMapSelected);

        PopulateMapDropdown();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        confirmButton.interactable = true;
    }

    public void Hide() => gameObject.SetActive(false);

    public void SetConfirmInteractable(bool interactable)
        => confirmButton.interactable = interactable;

    private void OnConfirmClicked()
    {
        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName)) return;

        string playerName = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        confirmButton.interactable = false;
        OnCreateRoomRequested?.Invoke(roomName, playerName, ResolveMap(mapDropdown.value));
    }

    private void OnMapSelected(int index)
    {
        mapPreviewImage.sprite = GetMapSprite(index);
    }

    private void PopulateMapDropdown()
    {
        var options = new System.Collections.Generic.List<string> { "Random" };
        foreach (var map in maps) options.Add(map);
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(options);
        mapDropdown.SetValueWithoutNotify(0);
        mapPreviewImage.sprite = GetMapSprite(0);
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
