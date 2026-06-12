using InfimaGames.LowPolyShooterPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour, IEventListener<WeaponChangedEvent>, IEventListener<AmmoChangedEvent>
{
    [Header("Images")]
    [SerializeField] private Image bodyImage;
    [SerializeField] private Image magazineImage;
    [SerializeField] private Image scopeImage;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI currentAmmoText;
    [SerializeField] private TextMeshProUGUI reserveAmmoText;

    private void OnEnable()
    {
        EventBus<WeaponChangedEvent>.Register(this);
        EventBus<AmmoChangedEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<WeaponChangedEvent>.Unregister(this);
        EventBus<AmmoChangedEvent>.Unregister(this);
    }

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

    public void OnEvent(WeaponChangedEvent e)
    {
        SetSprites(e.Weapon.BodySprite, e.Weapon.MagazineSprite, e.Weapon.ScopeSprite);
        RefreshAmmo(e.Weapon.CurrentAmmo, e.Weapon.ReserveAmmo);
    }

    public void OnEvent(AmmoChangedEvent e)
    {
        RefreshAmmo(e.Current, e.Reserve);
    }
}
