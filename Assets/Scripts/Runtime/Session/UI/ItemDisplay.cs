using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour, IEventListener<ItemAcquireProgressEvent>
{
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup panel;

    private void OnEnable()
    {
        EventBus<ItemAcquireProgressEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<ItemAcquireProgressEvent>.Unregister(this);
    }

    private void Awake()
    {
        panel.alpha = 0f;
    }

    public void OnEvent(ItemAcquireProgressEvent e)
    {
        panel.alpha = e.InRange ? 1f : 0f;
        slider.value = e.Normalized;
    }
}
