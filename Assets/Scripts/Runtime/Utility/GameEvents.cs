using Fusion;
using UnityEngine;

// UI
public struct HealthChangedEvent
{
    public HealthChangedEvent(PlayerRef source, float current, float extra, float max)
    {
        Source = source;
        Current = current;
        Extra = extra;
        Max = max;
    }

    public readonly PlayerRef Source { get; }
    public readonly float Current { get; }
    public readonly float Extra { get; }
    public readonly float Max { get; }
}

public struct AmmoChangedEvent
{
    public AmmoChangedEvent(int current, int reserve)
    {
        Current = current;
        Reserve = reserve;
    }

    public readonly int Current { get; }
    public readonly int Reserve { get; }
}

public struct WeaponChangedEvent
{
    public WeaponChangedEvent(WeaponBase weapon)
    {
        //Player = target;
        Weapon = weapon;
    }

    //public readonly PlayerRef Player { get; }
    public readonly WeaponBase Weapon { get; }
}

public struct AimChangedEvent
{
    public AimChangedEvent(bool isAiming)
    {
        IsAiming = isAiming;
    }

    public readonly bool IsAiming { get; }
}

public struct AbilityChangedEvent
{
    public AbilityChangedEvent(Sprite icon, int charges)
    {
        Icon = icon;
        Charges = charges;
    }

    public readonly Sprite Icon { get; }
    public readonly int Charges { get; }
}

public struct AbilityStateChangedEvent
{
    public AbilityStateChangedEvent(float charges)
    {
        Charges = charges;
    }

    public readonly float Charges { get; }
}

// Action
public struct PlayerDiedEvent
{
    public PlayerDiedEvent(PlayerRef target)
    {
        Player = target;
    }

    public readonly PlayerRef Player { get; }
}

public struct PositionRevealedEvent
{
    public PositionRevealedEvent(Highlighter target, float duration)
    {
        Target = target;
        Duration = duration;
    }

    public readonly Highlighter Target { get; }
    public readonly float Duration { get; }
}

public struct ItemAcquireProgressEvent
{
    public ItemAcquireProgressEvent(float normalized, bool inRange)
    {
        Normalized = normalized;
        InRange = inRange;
    }

    public readonly float Normalized { get; }
    public readonly bool InRange { get; }
}
