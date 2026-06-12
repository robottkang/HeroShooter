public struct HealthChangedEvent
{
    public HealthChangedEvent(Health target, float current, float extra, float max)
    {
        Target = target;
        Current = current;
        Extra = extra;
        Max = max;
    }
    public Health Target;
    public float Current;
    public float Extra;
    public float Max;
}

public struct PlayerDiedEvent { }

public struct AmmoChangedEvent
{
    public AmmoChangedEvent(int current, int reserve)
    {
        Current = current;
        Reserve = reserve;
    }
    public int Current;
    public int Reserve;
}

public struct WeaponChangedEvent
{
    public WeaponChangedEvent(WeaponBase weapon)
    {
        Weapon = weapon;
    }
    public WeaponBase Weapon;
}

public struct PositionRevealedEvent
{
    public PositionRevealedEvent(Highlighter target, float duration)
    {
        Target = target;
        Duration = duration;
    }
    public Highlighter Target;
    public float Duration;
}

public struct ItemAcquireProgressEvent
{
    public ItemAcquireProgressEvent(float normalized, bool inRange)
    {
        Normalized = normalized;
        InRange = inRange;
    }
    public float Normalized;
    public bool InRange;
}
