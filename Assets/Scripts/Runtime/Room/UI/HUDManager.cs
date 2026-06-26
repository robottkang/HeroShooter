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
}
