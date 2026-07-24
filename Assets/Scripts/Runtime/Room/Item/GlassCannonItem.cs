using UnityEngine;

public class GlassCannonItem : ItemBase
{
    [SerializeField] private WeaponBase glassCannonPrefab;

    public override void Acquire(IItemInteractor player)
    {
        var health = player.Health;
        health.SetExtraHealth(0f);
        health.TakeDamage(Mathf.Max(0f, health.Current - 1f));

        var inventory = player.Inventory;
        var weapon = Instantiate(glassCannonPrefab);
        inventory.AddWeapon(weapon);
        inventory.Equip(inventory.Weapons.Count - 1);

        RPC_Despawn();
    }
}
