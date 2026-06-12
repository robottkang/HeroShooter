using UnityEngine;
using TMPro;

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

    private void SetHealth(float current)
    {
        if (currentHealthText != null)
            currentHealthText.text = Mathf.CeilToInt(current).ToString("D3");
    }

    public void OnEvent(HealthChangedEvent e)
    {
        if (!e.Target.TryGetComponent(out PlayerController _)) return;
        SetHealth(e.Current + e.Extra);
    }
}
