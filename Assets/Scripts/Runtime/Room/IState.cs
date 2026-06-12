public abstract class BaseState
{
    protected PlayerController _player;

    protected BaseState(PlayerController player)
    {
        _player = player;
    }

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}
