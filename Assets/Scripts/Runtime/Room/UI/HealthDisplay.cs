using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour, IEventListener<HealthChangedEvent>
{
    [SerializeField] private TextMeshProUGUI currentHealthText;

    private void OnEnable()
    {
        EventBus<HealthChangedEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<HealthChangedEvent>.Unregister(this);
    }

    public void OnEvent(HealthChangedEvent e)
    {
        currentHealthText.text = Mathf.CeilToInt(e.Current + e.Extra).ToString("D3");
    }
}
