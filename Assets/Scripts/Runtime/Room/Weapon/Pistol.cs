using UnityEngine;

public class Pistol : WeaponBase
{
#if UNITY_EDITOR
    protected virtual void Reset()
    {
        damage = 20f;
        magazineSize = 12;
        reserveAmmo = 36;
        roundsPerMinute = 300f;
        range = 80f;
        reloadTime = 1.5f;
        isAutomatic = false;
    }
#endif
}
