using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] FirstPersonController fpc;

    Animator _anim;
    CharacterController _cc;

    static readonly int VelocityXId   = Animator.StringToHash("VelocityX");
    static readonly int VelocityYId   = Animator.StringToHash("VelocityY");
    static readonly int IsCrouchingId = Animator.StringToHash("IsCrouching");
    static readonly int IsGroundedId  = Animator.StringToHash("IsGrounded");
    static readonly int JumpTriggerId = Animator.StringToHash("JumpTrigger");
    static readonly int IsDeadId      = Animator.StringToHash("IsDead");

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _cc   = fpc.GetComponent<CharacterController>();
        fpc.OnJump += TriggerJump;
    }

    void OnDestroy()
    {
        if (fpc != null) fpc.OnJump -= TriggerJump;
    }

    void Update()
    {
        // 수평 이동 속도만 추출해 로컬 공간으로 변환
        Vector3 worldVel = _cc.velocity;
        worldVel.y = 0f;
        Vector3 localVel = fpc.transform.InverseTransformDirection(worldVel);

        // 0.1f dampTime으로 부드럽게 블렌딩
        _anim.SetFloat(VelocityXId,   localVel.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(VelocityYId,   localVel.z, 0.1f, Time.deltaTime);
        _anim.SetBool(IsCrouchingId,  fpc.IsCrouching);

        _anim.SetBool(IsGroundedId, fpc.IsGrounded);
    }

    void TriggerJump() => _anim.SetTrigger(JumpTriggerId);

    public void SetDead(bool isDead) => _anim.SetBool(IsDeadId, isDead);
}
