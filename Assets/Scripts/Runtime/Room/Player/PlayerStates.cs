using UnityEngine;

public class MoveIdleState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player) { }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player)
    {
        player.HandleMovement();
        player.HandleCrouchTransition();
    }
}

public class MoveWalkState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player) { }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player)
    {
        player.HandleMovement();
        player.HandleCrouchTransition();
    }
}

public class MoveSprintState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player) { }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player)
    {
        player.HandleMovement();
        player.HandleCrouchTransition();
    }
}

public class MoveCrouchState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player)
    {
        player.HandleCrouch(true);
    }

    public void OnStateExit(PlayerController player)
    {
        player.HandleCrouch(false);
    }

    public void OnStateUpdate(PlayerController player)
    {
        player.HandleMovement();
        player.HandleCrouchTransition();
    }
}

public class ActionIdleState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player) { }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player) { }
}

public class ActionAttackState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player) { }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player)
    {
        player.HandleAttack();
    }
}

public class ActionReloadState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player)
    {
        player.HandleReload();
    }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player) { }
}

public class ActionSwitchState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player) { }

    public void OnStateExit(PlayerController player) { }

    public void OnStateUpdate(PlayerController player) { }
}

public class ActionAcquireState : IBaseState<PlayerController>
{
    public void OnStateEnter(PlayerController player)
    {
        player.HandleAcquire(true);
    }

    public void OnStateExit(PlayerController player)
    {
        player.HandleAcquire(false);
    }

    public void OnStateUpdate(PlayerController player) { }
}
