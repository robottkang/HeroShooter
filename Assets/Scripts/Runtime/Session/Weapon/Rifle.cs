using UnityEngine;

public class Rifle : WeaponBase
{
#if UNITY_EDITOR
    protected virtual void Reset()
    {
        damage = 30f;
        magazineSize = 30;
        reserveAmmo = 90;
        roundsPerMinute = 600f;
        range = 200f;
        reloadTime = 2.2f;
        isAutomatic = true;
    }
#endif
}
