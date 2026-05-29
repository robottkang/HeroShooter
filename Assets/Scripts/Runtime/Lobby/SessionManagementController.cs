using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionManagementController : MonoBehaviour
{
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapPreviewImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Map Config")]
    [SerializeField] private string[] maps = { "Map1", "Map2" };
    [SerializeField] private Sprite[] mapPreviewSprites;
    [SerializeField] private Sprite randomPreviewSprite;

    private string _pendingMap;

    public delegate void OnCreateSessionHandler (string sessionName, string mapName);
    public event OnCreateSessionHandler OnCreateSessionRequested;
    public event Action OnBackRequested;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(OnBackClicked);
        mapDropdown.onValueChanged.AddListener(selectedIndex => OnMapSelected(selectedIndex));

        PopulateMapDropdown();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnConfirmClicked()
    {
        string sessionName = sessionNameInput.text.Trim();
        if (string.IsNullOrEmpty(sessionName)) return;

        OnCreateSessionRequested?.Invoke(sessionName, _pendingMap);

        confirmButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
    }

    private void OnBackClicked()
    {
        OnBackRequested?.Invoke();

        confirmButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
    }

    private void OnMapSelected(int index)
    {
        mapPreviewImage.sprite = GetMapSprite(index);
        _pendingMap = GetMapName(index);
    }

    private void PopulateMapDropdown()
    {
        var options = new System.Collections.Generic.List<string> { "Random" };
        foreach (var map in maps)
            options.Add(map);

        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(options);
        mapDropdown.SetValueWithoutNotify(0);
        mapPreviewImage.sprite = GetMapSprite(0);
    }

    private Sprite GetMapSprite(int dropdownIndex)
    {
        if (dropdownIndex == 0)
            return randomPreviewSprite;
        return mapPreviewSprites[dropdownIndex - 1];
    }

    private string GetMapName(int dropdownIndex)
    {
        if (dropdownIndex == 0)
            return maps[UnityEngine.Random.Range(0, maps.Length)];
        return maps[dropdownIndex - 1];
    }
}
