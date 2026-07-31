public interface IBaseState<T>
{
    public abstract void OnStateEnter(T sender);
    public abstract void OnStateUpdate(T sender);
    public abstract void OnStateExit(T sender);
}
