using UnityEngine;

public class SuperArmorItem : ItemBase
{
    [SerializeField] private float armorAmount = 100f;

    public override void Acquire(PlayerController player)
    {
        player.GetComponent<Health>().SetExtraHealth(armorAmount);
        Destroy(gameObject);
    }
}
