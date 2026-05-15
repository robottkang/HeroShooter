using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Photon.Pun;

public class FirstPersonController : MonoBehaviourPun, IPunObservable
{
    [Header("Input")]
    [SerializeField] private InputActionAsset actionAsset;

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

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float maxPitchAngle = 80f;
    [SerializeField] private Transform cameraHolder;

    private CharacterController _cc;
    private PhotonView _pv;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity;
    private float _xRotation;
    private bool _isCrouching;
    private float _jumpBufferTimer;
    private float _airSpeed = 5f;

    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private float _networkCamPitch;

    public event Action OnJump;
    public Vector2 MoveInput => _moveInput;
    public bool IsCrouching => _isCrouching;
    public bool IsGrounded => _cc.isGrounded;
    public bool IsSprinting => !_isCrouching && _sprintAction != null && _sprintAction.IsPressed();
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _pv = GetComponent<PhotonView>();

        _cc.height = standHeight;
        _cc.center = Vector3.up * (standHeight / 2f);

        if (_pv.IsMine)
        {
            var playerMap = actionAsset.FindActionMap("Player", true);
            _moveAction = playerMap.FindAction("Move", true);
            _lookAction = playerMap.FindAction("Look", true);
            _jumpAction = playerMap.FindAction("Jump", true);
            _sprintAction = playerMap.FindAction("Sprint", true);
            _crouchAction = playerMap.FindAction("Crouch", true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _cc.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!_pv.IsMine) return;
        
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _crouchAction.Enable();
    }

    private void OnDisable()
    {
        if (!_pv.IsMine) return;

        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();
        _crouchAction.Disable();
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
            _lookInput = _lookAction.ReadValue<Vector2>();

            HandleCrouch();
            HandleJumpBuffer();
            HandleLook();
            HandleMovement();
            HandleCrouchTransition();
        }
        else
        {
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 15f),
                Quaternion.Lerp(transform.rotation, _networkRotation, Time.deltaTime * 15f));

            if (cameraHolder != null)
            {
                var camRot = cameraHolder.localRotation;
                camRot = Quaternion.Lerp(camRot, Quaternion.Euler(_networkCamPitch, 0, 0), Time.deltaTime * 15f);
                cameraHolder.localRotation = camRot;
            }
        }
    }

    private void HandleCrouch()
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

    private void HandleJumpBuffer()
    {
        if (_jumpAction.WasPressedThisFrame() && !_isCrouching)
            _jumpBufferTimer = jumpBufferTime;

        _jumpBufferTimer -= Time.deltaTime;
    }

    private void HandleLook()
    {
        _xRotation -= _lookInput.y * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -maxPitchAngle, maxPitchAngle);

        cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(_lookInput.x * mouseSensitivity * Vector3.up);
    }

    private void HandleMovement()
    {
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        bool isSprinting = !_isCrouching && _sprintAction.IsPressed();
        float speed = _isCrouching ? crouchSpeed : (isSprinting ? runSpeed : walkSpeed);

        if (_cc.isGrounded)
            _airSpeed = speed;

        if (_jumpBufferTimer > 0f && _cc.isGrounded)
            TriggerJump();

        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        move *= _cc.isGrounded ? speed : _airSpeed;

        _verticalVelocity += gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    private void HandleCrouchTransition()
    {
        float targetHeight = _isCrouching ? crouchHeight : standHeight;
        float targetCamY   = _isCrouching ? crouchCameraY : standCameraY;

        _cc.height = Mathf.Lerp(_cc.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        _cc.center = Vector3.up * (_cc.height / 2f);

        Vector3 camPos = cameraHolder.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, crouchTransitionSpeed * Time.deltaTime);
        cameraHolder.localPosition = camPos;
    }

    private bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * _cc.height;
        float checkDistance = standHeight - _cc.height;
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDistance + 0.05f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(_xRotation);
            stream.SendNext(_isCrouching);
        }
        else
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _networkCamPitch = (float)stream.ReceiveNext();
            _isCrouching = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    private void RPC_Jump()
    {
        OnJump?.Invoke();
    }

    private void TriggerJump()
    {
        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        _jumpBufferTimer = 0f;
        OnJump?.Invoke();
        photonView.RPC(nameof(RPC_Jump), RpcTarget.Others);
    }
}
