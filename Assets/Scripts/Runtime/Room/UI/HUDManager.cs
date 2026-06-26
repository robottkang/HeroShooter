using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponDisplay weaponDisplay;

    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;

    // [Header("Skills")]
    // [SerializeField] private SkillDisplay skillDisplay;

    [Header("Others")]
    [SerializeField] private Crosshair crosshair;

    private PlayerController _playerController;

    public void Init(PlayerController playerController)
    {
        _playerController.OnAimingChanged += OnAimingChanged;
        healthDisplay.Init(playerController.GetComponent<Health>());
    }

    private void OnAimingChanged(bool isAiming)
    {
        if (isAiming)
            crosshair.Hide();
        else
            crosshair.Show();
    }
}
