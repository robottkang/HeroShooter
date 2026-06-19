using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCameraController cameraHolder;

    private Animator _anim;
    private CharacterController _cc;
    private Transform _spine;

    private static readonly int VelocityXId   = Animator.StringToHash("VelocityX");
    private static readonly int VelocityYId   = Animator.StringToHash("VelocityY");
    private static readonly int IsCrouchingId = Animator.StringToHash("IsCrouching");
    private static readonly int IsGroundedId  = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerId = Animator.StringToHash("JumpTrigger");
    private static readonly int IsDeadId      = Animator.StringToHash("IsDead");
    private static readonly int IsAiming      = Animator.StringToHash("IsAiming");

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _cc = playerController.GetComponent<CharacterController>();

        playerController.OnJump += TriggerJump;

        _spine = _anim.GetBoneTransform(HumanBodyBones.Spine);
    }

    private void OnDestroy()
    {
        if (playerController != null)
            playerController.OnJump -= TriggerJump;
    }

    private void Update()
    {
        Vector3 worldVel = _cc.velocity;
        worldVel.y = 0f;
        Vector3 localVel = playerController.transform.InverseTransformDirection(worldVel);

        _anim.SetFloat(VelocityXId, localVel.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(VelocityYId, localVel.z, 0.1f, Time.deltaTime);
        _anim.SetBool(IsAiming, playerController.IsAiming);
        _anim.SetBool(IsCrouchingId, playerController.IsCrouching);
        _anim.SetBool(IsGroundedId, playerController.IsGrounded);
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
