using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] TextMeshProUGUI hostNameText;
    [SerializeField] Button joinButton;

    public void Setup(string roomName, string hostName, System.Action onJoin)
    {
        roomNameText.text = roomName;
        hostNameText.text = hostName;
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onJoin?.Invoke());
    }
}
