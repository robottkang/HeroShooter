using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    private Animator _anim;
    private CharacterController _cc;

    static readonly int VelocityXId   = Animator.StringToHash("VelocityX");
    static readonly int VelocityYId   = Animator.StringToHash("VelocityY");
    static readonly int IsCrouchingId = Animator.StringToHash("IsCrouching");
    static readonly int IsGroundedId  = Animator.StringToHash("IsGrounded");
    static readonly int JumpTriggerId = Animator.StringToHash("JumpTrigger");
    static readonly int IsDeadId      = Animator.StringToHash("IsDead");

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _cc = playerController.GetComponent<CharacterController>();
        playerController.OnJump += TriggerJump;
    }

    private void OnDestroy()
    {
        if (playerController != null) playerController.OnJump -= TriggerJump;
    }

    private void Update()
    {
        Vector3 worldVel = _cc.velocity;
        worldVel.y = 0f;
        Vector3 localVel = playerController.transform.InverseTransformDirection(worldVel);

        _anim.SetFloat(VelocityXId, localVel.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(VelocityYId, localVel.z, 0.1f, Time.deltaTime);
        _anim.SetBool(IsCrouchingId, playerController.IsCrouching);

        _anim.SetBool(IsGroundedId, playerController.IsGrounded);
    }

    private void TriggerJump() => _anim.SetTrigger(JumpTriggerId);

    public void SetDead(bool isDead) => _anim.SetBool(IsDeadId, isDead);
}
