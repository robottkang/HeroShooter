using UnityEngine;

public interface IWeapon
{
    public abstract void Fire();

    public abstract void Reload();

    public abstract void FillAmmunition(int amount);

    public abstract void EjectCasing();
}
