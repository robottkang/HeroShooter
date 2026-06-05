using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponDisplay weaponDisplay;

    [Header("Health")]
    [SerializeField] private HealthDisplay healthDisplay;

    // [Header("Skills")]
    // [SerializeField] private SkillDisplay skillDisplay;

    private WeaponInventory _weaponInventory;
    private Health _health;

    private void Start()
    {
        _weaponInventory = FindFirstObjectByType<WeaponInventory>();
        _weaponInventory.OnWeaponChanged += OnWeaponChanged;
        OnWeaponChanged(_weaponInventory.CurrentWeapon);

        _health = FindFirstObjectByType<Health>();
        _health.OnHealthChanged += healthDisplay.SetHealth;
        healthDisplay.SetHealth(_health.Current, _health.Max);
    }

    private void OnDestroy()
    {
        _weaponInventory.OnWeaponChanged -= OnWeaponChanged;
        _health.OnHealthChanged -= healthDisplay.SetHealth;
    }

    private void OnWeaponChanged(WeaponBase weapon)
    {
        weaponDisplay.SetWeapon(weapon);
    }
}
