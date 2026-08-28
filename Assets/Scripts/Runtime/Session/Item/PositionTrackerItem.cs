using UnityEngine;

public class PositionTrackerItem : ItemBase
{
    [SerializeField] private float trackingDuration = 5f; 

    public override void Acquire(IItemInteractor player)
    {
        foreach (var h in FindObjectsByType<Highlighter>())
        {
            if (player.Highlighter == h) continue;
            EventBus<PositionRevealedEvent>.Raise(new PositionRevealedEvent(h, trackingDuration));
        }

        RPC_Despawn();
    }
}
