using UnityEngine;

public class StateMachine
{
    public StateMachine(BaseState initState)
    {
        currentState = initState;
        ChangeState(initState);
    }

    private BaseState currentState;

    public void ChangeState(BaseState newState)
    {
        currentState?.OnStateExit();
        currentState = newState;
        currentState?.OnStateEnter();
    }

    public void UpdateState()
    {
        currentState?.OnStateUpdate();
    }
}
