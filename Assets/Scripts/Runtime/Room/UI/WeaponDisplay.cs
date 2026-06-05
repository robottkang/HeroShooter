using UnityEngine;
using UnityEngine.UI;
using TMPro;
/*
EventBus 持失
EventBus
いOnWeaponFire
いOnWeaponChanage

WeaponBase {
Fire() => ... OnWeaponFire(currentAmmo, reserveAmmo);
}
WeaponInventory {
Chanage() => ... OnWeaponChanage(Weapon)
}
*/

public class WeaponDisplay : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image magazineImage;
    [SerializeField] private Image scopeImage;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI currentAmmoText;
    [SerializeField] private TextMeshProUGUI reserveAmmoText;

    public void RefreshAmmo(int current, int reserve)
    {
        currentAmmoText.text = current.ToString();
        reserveAmmoText.text = reserve.ToString();
    }

    public void SetSprites(Sprite body, Sprite magazine, Sprite scope)
    {
        bodyImage.sprite = body;
        magazineImage.sprite = magazine;
        scopeImage.sprite = scope;
    }
}
