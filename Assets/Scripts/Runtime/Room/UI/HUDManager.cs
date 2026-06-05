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

    private FirstPersonController _fpc;
    private WeaponInventory _weaponInventory;
    private WeaponBase _weapon;
    private Health _health;

    public void Init(FirstPersonController fpc, WeaponInventory weaponInventory, Health health)
    {
        _fpc = fpc;
        _fpc.OnAimingChanged += OnAimingChanged;

        _weaponInventory = weaponInventory;
        _weaponInventory.OnWeaponChanged += OnWeaponChanged;
        OnWeaponChanged(_weaponInventory.CurrentWeapon);

        _health = health;
        _health.OnHealthChanged += healthDisplay.SetHealth;
        healthDisplay.SetHealth(_health.Current, _health.Max);
    }

    private void OnDestroy()
    {
        _weaponInventory.OnWeaponChanged -= OnWeaponChanged;
        _health.OnHealthChanged -= healthDisplay.SetHealth;
        DetachWeapon();
    }

    private void OnAimingChanged(bool isAiming)
    {
        if (isAiming)
            crosshair.Hide();
        else
            crosshair.Show();
    }

    private void OnWeaponChanged(WeaponBase weapon)
    {
        DetachWeapon();

        _weapon = weapon;
        if (weapon == null) return;

        weaponDisplay.SetSprites(_weapon.BodySprite, _weapon.MagazineSprite, _weapon.ScopeSprite);

        weapon.OnAmmoChanged += weaponDisplay.RefreshAmmo;
        weaponDisplay.RefreshAmmo(weapon.CurrentAmmo, weapon.ReserveAmmo);
    }

    public void DetachWeapon()
    {
        if (_weapon == null) return;
        _weapon.OnAmmoChanged -= weaponDisplay.RefreshAmmo;
        _weapon = null;
    }
}
