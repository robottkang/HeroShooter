using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, IItemInteractor
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

    [Header("Reference")]
    [SerializeField] private Health health;
    [SerializeField] private WeaponInventory inventory;
    [SerializeField] private Highlighter highlighter;

    [Header("Look")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(1f, 1f);
    [SerializeField] private float maxPitchAngle = 80f;
    [SerializeField] private PlayerCameraController cameraHolder;

    [Networked, HideInInspector] private Vector3 NetworkedPosition { get; set; }
    [Networked, HideInInspector] private Quaternion NetworkedRotation { get; set; }
    [Networked, HideInInspector] public float CamPitch { get; private set; }
    [Networked, HideInInspector] public NetworkBool IsCrouching { get; private set; }
    [Networked, HideInInspector] public NetworkBool IsGrounded { get; private set; }
    [Networked, HideInInspector] public Vector2 MoveInput { get; private set; }
    [Networked, HideInInspector] public NetworkBool IsSprinting { get; private set; }
    [Networked, HideInInspector] public NetworkBool IsAiming { get; private set; }

    private CharacterController _cc;
    private AbilityBase _ability;

    private StateMachine<PlayerController> _moveFSM;
    private StateMachine<PlayerController> _actionFSM;
    private MoveState _currentMoveState;
    private ActionState _currentActionState;

    private InputAction _moveAction, _lookAction, _jumpAction, _sprintAction, _crouchAction, _reloadAction, _aimAction, _attackAction, _acquireAction, _abilityAction;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity;
    private float _jumpBufferTimer;
    private float _airSpeed = 5f;
    private bool _isSprinting;
    private bool _isAiming;
    private bool _isAcquiring;
    private bool _isNearItem;

    private bool IsActionBlocked => _isSprinting;
    private WeaponBase EquippedWeapon => inventory.CurrentWeapon;

    public event Action<bool> OnAimingChanged;
    public event Action OnJump;
    public CharacterController CC => _cc;
    public Health Health => health;
    public WeaponInventory Inventory => inventory;
    public Highlighter Highlighter => highlighter;
    public bool IsAcquiring => _isAcquiring;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float VerticalVelocity { get => _verticalVelocity; set => _verticalVelocity = value; }
    public float GravityMultiplier { get; set; } = 1f;

    public override void Spawned()
    {
        _cc = GetComponent<CharacterController>();
        _ability = GetComponent<AbilityBase>();

        _cc.height = standHeight;
        _cc.center = Vector3.up * (standHeight / 2f);

        if (Object.HasInputAuthority)
        {
            _moveFSM = new StateMachine<PlayerController>(this, new MoveIdleState());
            _actionFSM = new StateMachine<PlayerController>(this, new ActionIdleState());

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
        else
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("RemotePlayer_Other"));
            _cc.enabled = false;
            if (cameraHolder != null)
                cameraHolder.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        HandleAbility();
        HandleLook();
        HandleJumpBuffer();
        HandleScrollSwitch();
        HandleNumberKeySwitch();

        HandleMoveFSM();
        HandleActionFSM();

        _moveFSM.UpdateState();
        _actionFSM.UpdateState();

        HandleAiming();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        NetworkedPosition = transform.position;
        NetworkedRotation = transform.rotation;
        IsGrounded = _cc.isGrounded;
        MoveInput = _moveInput;
        IsSprinting = _isSprinting;
        IsAiming = _isAiming;
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
            camRot = Quaternion.Lerp(camRot, Quaternion.Euler(CamPitch, 0, 0), Time.deltaTime * 15f);
            cameraHolder.transform.localRotation = camRot;
        }
    }

    private void HandleMoveFSM()
    {
        // while ability that block movement is active
        if (_ability.IsActive && _ability.BlockFlags.HasFlag(AbilityBlockFlags.Move))
        {
            ChangeMoveState(MoveState.Idle);
            return;
        }

        // while crouch key(LCtrl) is pressed or player cannot stand up due to obstacle
        if (_crouchAction.IsPressed() || !CanStandUp())
        {
            ChangeMoveState(MoveState.Crouch);
            return;
        }

        // when wasd is not pressed
        if (_moveInput.sqrMagnitude < 0.01f)
        {
            ChangeMoveState(MoveState.Idle);
            return;
        }

        // while sprint key(LShift) is pressed
        if (_sprintAction.IsPressed())
        {
            ChangeMoveState(MoveState.Sprint);
            return;
        }

        // when just wasd is pressed
        ChangeMoveState(MoveState.Walk);
    }

    private void HandleActionFSM()
    {
        // while ability that block action is active
        if (IsActionBlocked || (_ability.IsActive && _ability.BlockFlags.HasFlag(AbilityBlockFlags.Action)))
        {
            ChangeActionState(ActionState.Idle);
            return;
        }

        // while weapon is switching
        if (inventory.IsSwitching)
        {
            ChangeActionState(ActionState.Switch);
            return;
        }

        // while weapon is reloading or reload key(R) is pressed
        if (EquippedWeapon.IsReloading || _reloadAction.WasPressedThisFrame())
        {
            ChangeActionState(ActionState.Reload);
            return;
        }

        // while player acquire key(E) is pressed in range of item
        if (_isNearItem && _acquireAction.IsPressed())
        {
            ChangeActionState(ActionState.Acquire);
            return;
        }

        // while attack key(LMB) is pressed and weapon has ammo
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
                _moveFSM.ChangeState(new MoveIdleState());
                break;
            case MoveState.Walk:
                _moveFSM.ChangeState(new MoveWalkState());
                break;
            case MoveState.Sprint:
                _moveFSM.ChangeState(new MoveSprintState());
                break;
            case MoveState.Crouch:
                _moveFSM.ChangeState(new MoveCrouchState());
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
                _actionFSM.ChangeState(new ActionIdleState());
                break;
            case ActionState.Attack:
                _actionFSM.ChangeState(new ActionAttackState());
                break;
            case ActionState.Reload:
                _actionFSM.ChangeState(new ActionReloadState());
                break;
            case ActionState.Switch:
                _actionFSM.ChangeState(new ActionSwitchState());
                break;
            case ActionState.Acquire:
                _actionFSM.ChangeState(new ActionAcquireState());
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void HandleJumpBuffer()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (_jumpAction.WasPressedThisFrame())
        {
            _jumpBufferTimer = jumpBufferTime;
        }
    }

    private void HandleLook()
    {
        CamPitch -= _lookInput.y * mouseSensitivity.x;
        CamPitch = Mathf.Clamp(CamPitch, -maxPitchAngle, maxPitchAngle);

        cameraHolder.transform.localRotation = Quaternion.Euler(CamPitch, 0f, 0f);
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
            inventory.EquipPrev();
        else if (scroll < 0f)
            inventory.EquipNext();
    }

    private void HandleNumberKeySwitch()
    {
        var kb = Keyboard.current;
        for (var i = 0; i < 9; i += 1)
        {
            if (kb[Key.Digit1 + i].wasPressedThisFrame)
            {
                inventory.Equip(i);
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
        if (_abilityAction.WasPressedThisFrame())
            _ability.TryUse(this);
    }

    public bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * _cc.height;
        float checkDistance = standHeight - _cc.height;
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDistance + 0.05f);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
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
