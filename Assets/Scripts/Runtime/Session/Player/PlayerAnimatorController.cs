using Fusion;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCameraController cameraHolder;

    private Animator _anim;
    private Transform _spine;

    private static readonly int VelocityXId = Animator.StringToHash("DirectionX");
    private static readonly int VelocityYId = Animator.StringToHash("DirectionY");
    private static readonly int MovementId = Animator.StringToHash("Movement");
    private static readonly int IsCrouchingId = Animator.StringToHash("IsCrouching");
    private static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerId = Animator.StringToHash("JumpTrigger");
    private static readonly int IsDeadId = Animator.StringToHash("IsDead");
    private static readonly int IsAimingId = Animator.StringToHash("IsAiming");
    private static readonly int IsRuningId = Animator.StringToHash("IsRuning");

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        playerController.OnJump += TriggerJump;
        _spine = _anim.GetBoneTransform(HumanBodyBones.Spine);
    }

    private void OnDestroy()
    {
        if (playerController != null)
            playerController.OnJump -= TriggerJump;
    }

    public void Update()
    {
        var moveDir = playerController.MoveInput.normalized;

        _anim.SetFloat(VelocityXId, moveDir.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(VelocityYId, moveDir.y, 0.1f, Time.deltaTime);
        _anim.SetFloat(MovementId, moveDir.magnitude, 0.1f, Time.deltaTime);
        _anim.SetBool(IsAimingId, playerController.IsAiming);
        _anim.SetBool(IsCrouchingId, playerController.IsCrouching);
        _anim.SetBool(IsGroundedId, playerController.IsGrounded);
        _anim.SetBool(IsRuningId, playerController.IsSprinting);
    }

    private void LateUpdate()
    {
        Vector3 spineEuler = _spine.localRotation.eulerAngles;
        spineEuler.x = playerController.CamPitch / 2;
        spineEuler.z = playerController.CamPitch / 2;
        _spine.localRotation = Quaternion.Euler(spineEuler);
    }

    private void TriggerJump()
    {
        _anim.SetTrigger(JumpTriggerId);
    }

    public void SetDead(bool isDead)
    {
        _anim.SetBool(IsDeadId, isDead);
    }
}
