using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentHealthText;
    [SerializeField] private TextMeshProUGUI maxHealthText;

    public void SetHealth(float current, float max)
    {
        if (currentHealthText != null)
            currentHealthText.text = Mathf.CeilToInt(current).ToString("D3");
        if (maxHealthText != null)
            maxHealthText.text = Mathf.CeilToInt(max).ToString("D3");
    }
}
