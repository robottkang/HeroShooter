using UnityEngine;
using UnityEngine.InputSystem;

// Required hierarchy:
//   Player  (CharacterController + FirstPersonController)
//   └── CameraHolder  ← assign to cameraHolder field
//        └── Main Camera
//
// Inspector: actionAsset 필드에 Assets/InputSystem_Actions.inputactions 드래그
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset actionAsset;

    [Header("Speed")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float runSpeed = 9f;
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("Jump & Gravity")]
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float gravity = -20f;
    [SerializeField] float jumpBufferTime = 0.2f;

    [Header("Crouch")]
    [SerializeField] float standHeight = 2f;
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float crouchTransitionSpeed = 12f;
    [SerializeField] float standCameraY = 1.8f;
    [SerializeField] float crouchCameraY = 0.8f;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float maxPitchAngle = 80f;
    [SerializeField] Transform cameraHolder;

    CharacterController _cc;

    InputAction _moveAction;
    InputAction _lookAction;
    InputAction _jumpAction;
    InputAction _sprintAction;
    InputAction _crouchAction;

    Vector2 _moveInput;
    Vector2 _lookInput;
    float _verticalVelocity;
    float _xRotation;
    bool _isCrouching;
    float _jumpBufferTimer;
    float _airSpeed;

    public event System.Action OnJump;
    public Vector2 MoveInput    => _moveInput;
    public bool IsCrouching     => _isCrouching;
    public bool IsGrounded      => _cc != null && _cc.isGrounded;
    public bool IsSprinting     => !_isCrouching && _sprintAction != null && _sprintAction.IsPressed();
    public float WalkSpeed      => walkSpeed;
    public float RunSpeed       => runSpeed;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        var playerMap = actionAsset.FindActionMap("Player", throwIfNotFound: true);
        _moveAction   = playerMap.FindAction("Move",   throwIfNotFound: true);
        _lookAction   = playerMap.FindAction("Look",   throwIfNotFound: true);
        _jumpAction   = playerMap.FindAction("Jump",   throwIfNotFound: true);
        _sprintAction = playerMap.FindAction("Sprint", throwIfNotFound: true);
        _crouchAction = playerMap.FindAction("Crouch", throwIfNotFound: true);

        _cc.height = standHeight;
        _cc.center = Vector3.up * (standHeight / 2f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _crouchAction.Enable();
    }

    void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();
        _crouchAction.Disable();
    }

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        HandleCrouch();
        HandleJumpBuffer();
        HandleLook();
        HandleMovement();
        HandleCrouchTransition();
    }

    void HandleCrouch()
    {
        if (_crouchAction.IsPressed())
        {
            _isCrouching = true;
        }
        else if (_isCrouching && CanStandUp())
        {
            _isCrouching = false;
        }
    }

    // 이 프레임에 점프 버튼을 눌렀을 때만 버퍼 타이머를 설정.
    // 타이머가 0 이하로 내려가면 해당 입력은 완전히 소멸 — 착지 후 자동점프 없음.
    void HandleJumpBuffer()
    {
        if (_jumpAction.WasPressedThisFrame() && !_isCrouching)
            _jumpBufferTimer = jumpBufferTime;

        _jumpBufferTimer -= Time.deltaTime;
    }

    void HandleLook()
    {
        _xRotation -= _lookInput.y * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -maxPitchAngle, maxPitchAngle);

        cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * _lookInput.x * mouseSensitivity);
    }

    void HandleMovement()
    {
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        bool isSprinting = !_isCrouching && _sprintAction.IsPressed();
        float speed = _isCrouching ? crouchSpeed : (isSprinting ? runSpeed : walkSpeed);

        // 지상에 있는 동안 공중 속도를 계속 갱신해두다가, 이륙 순간 그 값이 고정됨
        if (_cc.isGrounded)
            _airSpeed = speed;

        if (_jumpBufferTimer > 0f && _cc.isGrounded)
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpBufferTimer = 0f;
            OnJump?.Invoke();
        }

        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        move *= _cc.isGrounded ? speed : _airSpeed;

        _verticalVelocity += gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    void HandleCrouchTransition()
    {
        float targetHeight = _isCrouching ? crouchHeight : standHeight;
        float targetCamY   = _isCrouching ? crouchCameraY : standCameraY;

        _cc.height = Mathf.Lerp(_cc.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        _cc.center = Vector3.up * (_cc.height / 2f);

        Vector3 camPos = cameraHolder.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, crouchTransitionSpeed * Time.deltaTime);
        cameraHolder.localPosition = camPos;
    }

    bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * _cc.height;
        float checkDistance = standHeight - _cc.height;
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDistance + 0.05f);
    }
}
