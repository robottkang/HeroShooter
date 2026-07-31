using UnityEngine;

public class SessionReadyState : IBaseState<SessionManager>
{
    public void OnStateEnter(SessionManager session) { }
    public void OnStateExit(SessionManager session) { }
    public void OnStateUpdate(SessionManager session) { }
}

public class SessionPlayingState : IBaseState<SessionManager>
{
    public void OnStateEnter(SessionManager session) { }
    public void OnStateExit(SessionManager session) { }
    public void OnStateUpdate(SessionManager session) { }
}

public class SessionResultState : IBaseState<SessionManager>
{
    public void OnStateEnter(SessionManager session) { }
    public void OnStateExit(SessionManager session) { }
    public void OnStateUpdate(SessionManager session) { }
}
