using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour/*NetworkBehaviour*/, IItemInteractor
{
    private enum MoveState { Idle, Walk, Sprint, Crouch }
    private enum ActionState { Idle, Attack, Reload, Switch, Acquire }

    [Header("Input")]
    [SerializeField] private InputActionAsset actionAsset;

    [Header("Body")]
    [SerializeField] private Transform remoteBody;
    [SerializeField] private Transform localBody;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 12f;
    [SerializeField] private float standCameraY = 1.8f;
    [SerializeField] private float crouchCameraY = 0.8f;

    [Header("Weapon")]
    [SerializeField] private WeaponInventory weaponInventory;

    [Header("Ability")]
    [SerializeField] private AbilityBase ability;

    [Header("Look")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(1f, 1f);
    [SerializeField] private float maxPitchAngle = 80f;
    [SerializeField] private PlayerCameraController cameraHolder;

    [Networked] private Vector3 NetworkedPosition { get; set; }
    [Networked] private Quaternion NetworkedRotation { get; set; }
    [Networked] private float NetworkedCamPitch { get; set; }
    [Networked] public NetworkBool IsCrouching { get; private set; }

    private CharacterController _cc;

    private StateMachine _moveFSM;
    private StateMachine _actionFSM;
    private MoveState _currentMoveState;
    private ActionState _currentActionState;

    private InputAction _moveAction, _lookAction, _jumpAction, _sprintAction, _crouchAction, _reloadAction, _aimAction, _attackAction, _acquireAction, _abilityAction;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity;
    private float _xRotation;
    private float _jumpBufferTimer;
    private float _airSpeed = 5f;
    private bool _isSprinting;
    private bool _isAiming;
    private bool _isAcquiring;
    private bool _isNearItem;

    private bool IsActionBlocked => _isSprinting;
    private WeaponBase EquippedWeapon => weaponInventory.CurrentWeapon;

    public event Action<bool> OnAimingChanged;
    public event Action OnJump;
    public CharacterController CC => _cc;
    public Vector2 MoveInput => _moveInput;
    public bool IsGrounded => _cc.isGrounded;
    public bool IsSprinting => _isSprinting;
    public bool IsAcquiring => _isAcquiring;
    public bool IsAiming => _isAiming;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float VerticalVelocity { get => _verticalVelocity; set => _verticalVelocity = value; }
    public float GravityMultiplier { get; set; } = 1f;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        _cc.height = standHeight;
        _cc.center = Vector3.up * (standHeight / 2f);

        _moveFSM = new StateMachine(new MoveIdleState(this));
        _actionFSM = new StateMachine(new ActionIdleState(this));

        var playerMap = actionAsset.FindActionMap("Player", true);
        _moveAction = playerMap.FindAction("Move", true);
        _lookAction = playerMap.FindAction("Look", true);
        _jumpAction = playerMap.FindAction("Jump", true);
        _sprintAction = playerMap.FindAction("Sprint", true);
        _crouchAction = playerMap.FindAction("Crouch", true);
        _reloadAction = playerMap.FindAction("Reload", true);
        _attackAction = playerMap.FindAction("Attack", true);
        _aimAction = playerMap.FindAction("Aim", true);
        _acquireAction = playerMap.FindAction("Acquire", true);
        _abilityAction = playerMap.FindAction("Ability", true);

        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _crouchAction.Enable();
        _reloadAction.Enable();
        _attackAction.Enable();
        _aimAction.Enable();
        _acquireAction.Enable();
        _abilityAction.Enable();
    }

    private void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        HandleLook();
        HandleJumpBuffer();
        HandleScrollSwitch();
        HandleNumberKeySwitch();

        HandleMoveFSM();
        HandleActionFSM();

        _moveFSM.UpdateState();
        _actionFSM.UpdateState();

        HandleAiming();
        /*
        HandleMovement();
        HandleCrouch();

        HandleCrouchTransition();
        HandleReload();
        HandleAiming();
        HandleAttack();
        HandleAbility();
        */
    }

    /*
    public override void Spawned()
    {
        _cc = GetComponent<CharacterController>();

        _cc.height = standHeight;
        _cc.center = Vector3.up * (standHeight / 2f);

        if (Object.HasInputAuthority)
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("RemotePlayer_Self"));

            var playerMap = actionAsset.FindActionMap("Player", true);
            _moveAction = playerMap.FindAction("Move", true);
            _lookAction = playerMap.FindAction("Look", true);
            _jumpAction = playerMap.FindAction("Jump", true);
            _sprintAction = playerMap.FindAction("Sprint", true);
            _crouchAction = playerMap.FindAction("Crouch", true);

            _moveAction.Enable();
            _lookAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
            _crouchAction.Enable();
        }
        else
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("RemotePlayer_Other"));
            _cc.enabled = false;
            if (cameraHolder != null)
                cameraHolder.gameObject.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        HandleCrouch();
        HandleJumpBuffer();
        HandleLook();
        HandleMovement();
        HandleCrouchTransition();

        NetworkedPosition = transform.position;
        NetworkedRotation = transform.rotation;
        NetworkedCamPitch = _xRotation;
    }

    public override void Render()
    {
        if (Object.HasInputAuthority) return;

        transform.SetPositionAndRotation(
            Vector3.Lerp(transform.position, NetworkedPosition, Time.deltaTime * 15f),
            Quaternion.Lerp(transform.rotation, NetworkedRotation, Time.deltaTime * 15f));

        if (cameraHolder != null)
        {
            var camRot = cameraHolder.transform.localRotation;
            camRot = Quaternion.Lerp(camRot, Quaternion.Euler(NetworkedCamPitch, 0, 0), Time.deltaTime * 15f);
            cameraHolder.transform.localRotation = camRot;
        }
    }*/

    private void HandleMoveFSM()
    {
        if (_crouchAction.IsPressed() || !CanStandUp()) // when press crouch
        {
            ChangeMoveState(MoveState.Crouch);
            return;
        }

        if (_moveInput.sqrMagnitude < 0.01f) // when stop
        {
            ChangeMoveState(MoveState.Idle);
            return;
        }

        if (_sprintAction.IsPressed()) // when press sprint
        {
            ChangeMoveState(MoveState.Sprint);
            return;
        }

        ChangeMoveState(MoveState.Walk);
    }

    private void HandleActionFSM()
    {
        if (IsActionBlocked)
        {
            ChangeActionState(ActionState.Idle);
            return;
        }

        if (weaponInventory.IsSwitching)
        {
            ChangeActionState(ActionState.Switch);
            return;
        }

        if (EquippedWeapon.IsReloading || _reloadAction.WasPressedThisFrame())
        {
            ChangeActionState(ActionState.Reload);
            return;
        }

        if (_isNearItem && _acquireAction.IsPressed())
        {
            ChangeActionState(ActionState.Acquire);
            return;
        }

        bool shouldFire = (EquippedWeapon.IsAutomatic && EquippedWeapon.CurrentAmmo > 0) ?
            _attackAction.IsPressed() : _attackAction.WasPressedThisFrame();
        if (shouldFire)
        {
            ChangeActionState(ActionState.Attack);
            return;
        }

        ChangeActionState(ActionState.Idle);
    }

    private void ChangeMoveState(MoveState newState)
    {
        if (_currentMoveState == newState) return;
        _currentMoveState = newState;

        switch (newState)
        {
            case MoveState.Idle:
                _moveFSM.ChangeState(new MoveIdleState(this));
                break;
            case MoveState.Walk:
                _moveFSM.ChangeState(new MoveWalkState(this));
                break;
            case MoveState.Sprint:
                _moveFSM.ChangeState(new MoveSprintState(this));
                break;
            case MoveState.Crouch:
                _moveFSM.ChangeState(new MoveCrouchState(this));
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void ChangeActionState(ActionState newState)
    {
        if (_currentActionState == newState) return;
        _currentActionState = newState;

        switch (newState)
        {
            case ActionState.Idle:
                _actionFSM.ChangeState(new ActionIdleState(this));
                break;
            case ActionState.Attack:
                _actionFSM.ChangeState(new ActionAttackState(this));
                break;
            case ActionState.Reload:
                _actionFSM.ChangeState(new ActionReloadState(this));
                break;
            case ActionState.Switch:
                _actionFSM.ChangeState(new ActionSwitchState(this));
                break;
            case ActionState.Acquire:
                _actionFSM.ChangeState(new ActionAcquireState(this));
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void HandleJumpBuffer()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (_jumpAction.WasPressedThisFrame() && !IsCrouching)
        {
            _jumpBufferTimer = jumpBufferTime;
        }
    }

    private void HandleLook()
    {
        _xRotation -= _lookInput.y * mouseSensitivity.x;
        _xRotation = Mathf.Clamp(_xRotation, -maxPitchAngle, maxPitchAngle);

        cameraHolder.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(_lookInput.x * mouseSensitivity.y * Vector3.up);
    }

    public void HandleMovement()
    {
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _isSprinting = !IsCrouching && !EquippedWeapon.IsReloading && _sprintAction.IsPressed();
        float speed = IsCrouching ? crouchSpeed : (_isSprinting ? runSpeed : walkSpeed);

        if (_cc.isGrounded)
            _airSpeed = speed;

        if (_jumpBufferTimer > 0f && _cc.isGrounded)
            TriggerJump();

        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        move *= _cc.isGrounded ? speed : _airSpeed;

        _verticalVelocity += gravity * GravityMultiplier * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    public void HandleCrouch(bool isCrouching)
    {
        IsCrouching = isCrouching;
    }

    public void HandleCrouchTransition()
    {
        float targetHeight = IsCrouching ? crouchHeight : standHeight;
        float targetCamY = IsCrouching ? crouchCameraY : standCameraY;

        _cc.height = Mathf.Lerp(_cc.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        _cc.center = Vector3.up * (_cc.height / 2f);

        Vector3 camPos = cameraHolder.transform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, crouchTransitionSpeed * Time.deltaTime);
        cameraHolder.transform.localPosition = camPos;
    }

    private void HandleAiming()
    {
        bool canAim = (_currentMoveState is not MoveState.Sprint)
            && (_currentActionState is ActionState.Idle or ActionState.Attack);

        bool shouldAim = canAim && _aimAction.IsPressed();
        if (shouldAim != _isAiming)
        {
            _isAiming = shouldAim;
            OnAimingChanged?.Invoke(_isAiming);
            cameraHolder.SetFieldOfView(_isAiming);
        }
    }

    public void HandleReload()
    {
        EquippedWeapon.Reload();
    }

    public void HandleAttack()
    {
        EquippedWeapon.Fire(cameraHolder.LocalCamera);
    }

    private void HandleScrollSwitch()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f)
            weaponInventory.EquipPrev();
        else if (scroll < 0f)
            weaponInventory.EquipNext();
    }

    private void HandleNumberKeySwitch()
    {
        var kb = Keyboard.current;
        for (var i = 0; i < 9; i += 1)
        {
            if (kb[Key.Digit1 + i].wasPressedThisFrame)
            {
                weaponInventory.Equip(i);
                break;
            }
        }
    }

    public void HandleAcquire(bool isAcquiring)
    {
        _isAcquiring = isAcquiring;
    }

    public void OnNearItem(bool isNear)
    {
        _isNearItem = isNear;
    }

    private void HandleAbility()
    {
        if (ability == null) return;
        if (_abilityAction.WasPressedThisFrame())
            ability.TryUse(this);
    }

    public bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * _cc.height;
        float checkDistance = standHeight - _cc.height;
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDistance + 0.05f);
    }

    //[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Jump()
    {
        OnJump?.Invoke();
    }

    private void TriggerJump()
    {
        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        _jumpBufferTimer = 0f;
        RPC_Jump();
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
