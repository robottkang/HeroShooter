public interface IItemInteractor
{
    bool IsAcquiring { get; }
    void OnNearItem(bool isNear);
}
