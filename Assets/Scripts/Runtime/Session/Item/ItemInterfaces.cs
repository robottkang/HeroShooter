public interface IItemInteractor
{
    public bool IsAcquiring { get; }
    public void OnNearItem(bool isNear);
    public Health Health { get; }
    public WeaponInventory Inventory { get; }
    public Highlighter Highlighter { get; }
}
