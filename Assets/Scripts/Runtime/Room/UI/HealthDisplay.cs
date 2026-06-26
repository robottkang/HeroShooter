using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour, IEventListener<HealthChangedEvent>
{
    [SerializeField] private TextMeshProUGUI currentHealthText;

    private Health _health;

    public void Init(Health health)
    {
        _health = health;
    }

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
        if (_health == null || e.Target != _health) return;
        if (currentHealthText != null)
            currentHealthText.text = Mathf.CeilToInt(e.Current + e.Extra).ToString("D3");
    }
}
