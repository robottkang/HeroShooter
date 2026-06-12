using UnityEngine;

public class PositionTrackerItem : ItemBase
{
    [SerializeField] private float trackingDuration = 5f; 

    public override void Acquire(PlayerController player)
    {
        foreach (var p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (player == p) continue;
            var h = p.GetComponentInChildren<Highlighter>();
            EventBus<PositionRevealedEvent>.Raise(new PositionRevealedEvent(h, trackingDuration));
        }

        Destroy(gameObject);
    }
}
