using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;

    public void SetHealth(float current, float max)
    {
        if (healthText != null)
            healthText.text = Mathf.CeilToInt(current).ToString("D3");
    }
}
