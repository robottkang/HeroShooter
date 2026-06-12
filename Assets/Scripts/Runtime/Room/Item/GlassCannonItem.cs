using UnityEngine;

public class GlassCannonItem : ItemBase
{
    [SerializeField] private WeaponBase glassCannonPrefab;

    public override void Acquire(PlayerController player)
    {
        var health = player.GetComponent<Health>();
        health.SetExtraHealth(0f);
        health.TakeDamage(Mathf.Max(0f, health.Current - 1f));

        var inventory = player.GetComponentInChildren<WeaponInventory>();
        var weapon = Instantiate(glassCannonPrefab);
        inventory.AddWeapon(weapon);
        inventory.Equip(inventory.Weapons.Count - 1);

        Destroy(gameObject);
    }
}
