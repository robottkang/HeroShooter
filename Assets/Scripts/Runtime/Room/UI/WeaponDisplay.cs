using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponDisplay : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image magazineImage;
    [SerializeField] private Image scopeImage;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI currentAmmoText;
    [SerializeField] private TextMeshProUGUI reserveAmmoText;

    private WeaponBase _weapon;

    public void SetWeapon(WeaponBase weapon)
    {
        Detach();

        _weapon = weapon;
        if (_weapon == null) return;

        ApplySprite(bodyImage, _weapon.BodySprite);
        ApplySprite(magazineImage, _weapon.MagazineSprite);

        if (scopeImage != null)
        {
            bool hasScope = _weapon.ScopeSprite != null;
            scopeImage.gameObject.SetActive(hasScope);
            if (hasScope)
                scopeImage.sprite = _weapon.ScopeSprite;
        }

        _weapon.OnAmmoChanged += RefreshAmmo;
        RefreshAmmo(_weapon.CurrentAmmo, _weapon.ReserveAmmo);
    }

    public void Detach()
    {
        if (_weapon == null) return;
        _weapon.OnAmmoChanged -= RefreshAmmo;
        _weapon = null;
    }

    private void RefreshAmmo(int current, int reserve)
    {
        if (currentAmmoText != null) currentAmmoText.text = current.ToString();
        if (reserveAmmoText != null) reserveAmmoText.text = reserve.ToString();
    }

    private static void ApplySprite(Image image, Sprite sprite)
    {
        if (image == null) return;
        image.sprite = sprite;
    }
}
