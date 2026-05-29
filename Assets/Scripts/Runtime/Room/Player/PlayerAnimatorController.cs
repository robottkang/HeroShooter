using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FirstPersonController fpc;

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
        _cc = fpc.GetComponent<CharacterController>();
        fpc.OnJump += TriggerJump;
    }

    private void OnDestroy()
    {
        if (fpc != null) fpc.OnJump -= TriggerJump;
    }

    private void Update()
    {
        Vector3 worldVel = _cc.velocity;
        worldVel.y = 0f;
        Vector3 localVel = fpc.transform.InverseTransformDirection(worldVel);

        _anim.SetFloat(VelocityXId, localVel.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(VelocityYId, localVel.z, 0.1f, Time.deltaTime);
        _anim.SetBool(IsCrouchingId, fpc.IsCrouching);

        _anim.SetBool(IsGroundedId, fpc.IsGrounded);
    }

    private void TriggerJump() => _anim.SetTrigger(JumpTriggerId);

    public void SetDead(bool isDead) => _anim.SetBool(IsDeadId, isDead);
}
