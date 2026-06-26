public interface IItemInteractor
{
    public bool IsAcquiring { get; }
    public void OnNearItem(bool isNear);
}

public interface IHealthInteractor
{

}
