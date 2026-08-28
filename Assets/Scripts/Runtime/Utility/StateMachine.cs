using UnityEngine;

public class StateMachine<T>
{
    private T _sender;
    private IBaseState<T> _currentState;

    public StateMachine(T sender, IBaseState<T> initState)
    {
        _sender = sender;
        ChangeState(initState);
    }

    public void ChangeState(IBaseState<T> newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        _currentState?.OnStateExit(_sender);
        _currentState = newState;
        _currentState?.OnStateEnter(_sender);
    }

    public void UpdateState()
    {
        _currentState?.OnStateUpdate(_sender);
    }
}
