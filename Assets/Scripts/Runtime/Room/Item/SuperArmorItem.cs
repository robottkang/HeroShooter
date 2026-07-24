using UnityEngine;

public class SuperArmorItem : ItemBase
{
    [SerializeField] private float armorAmount = 100f;

    public override void Acquire(IItemInteractor player)
    {
        player.Health.SetExtraHealth(armorAmount);

        RPC_Despawn();
    }
}
