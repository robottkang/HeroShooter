using UnityEngine;

public class MoveIdleState : BaseState
{
    public MoveIdleState(PlayerController player) : base(player) { }

    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void OnStateUpdate()
    {
        _player.HandleMovement();
        _player.HandleCrouchTransition();
    }
}

public class MoveWalkState : BaseState
{
    public MoveWalkState(PlayerController player) : base(player) { }

    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void OnStateUpdate()
    {
        _player.HandleMovement();
        _player.HandleCrouchTransition();
    }
}

public class MoveSprintState : BaseState
{
    public MoveSprintState(PlayerController player) : base(player) { }

    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void OnStateUpdate()
    {
        _player.HandleMovement();
        _player.HandleCrouchTransition();
    }
}

public class MoveCrouchState : BaseState
{
    public MoveCrouchState(PlayerController player) : base(player) { }

    public override void OnStateEnter()
    {
        _player.HandleCrouch(true);
    }

    public override void OnStateExit()
    {
        _player.HandleCrouch(false);
    }

    public override void OnStateUpdate()
    {
        _player.HandleMovement();
        _player.HandleCrouchTransition();
    }
}

public class ActionIdleState : BaseState
{
    public ActionIdleState(PlayerController player) : base(player) { }

    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void OnStateUpdate() { }
}

public class ActionAttackState : BaseState
{
    public ActionAttackState(PlayerController player) : base(player) { }

    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void OnStateUpdate()
    {
        _player.HandleAttack();
    }
}

public class ActionReloadState : BaseState
{
    public ActionReloadState(PlayerController player) : base(player) { }

    public override void OnStateEnter()
    {
        _player.HandleReload();
    }

    public override void OnStateExit() { }

    public override void OnStateUpdate() { }
}

public class ActionSwitchState : BaseState
{
    public ActionSwitchState(PlayerController player) : base(player) { }

    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void OnStateUpdate() { }
}

public class ActionAcquireState : BaseState
{
    public ActionAcquireState(PlayerController player) : base(player) { }

    public override void OnStateEnter()
    {
        _player.HandleAcquire(true);
    }

    public override void OnStateExit()
    {
        _player.HandleAcquire(false);
    }

    public override void OnStateUpdate() { }
}
