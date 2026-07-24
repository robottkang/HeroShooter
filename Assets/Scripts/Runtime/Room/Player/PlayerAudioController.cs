using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAudioController : MonoBehaviour
{
    [Header("Footstep - Distance")]
    [SerializeField] private float walkStepDistance = 0.6f;
    [SerializeField] private float sprintStepDistance = 0.4f;
    [SerializeField] private float crouchStepDistance = 0.8f;

    [Header("Footstep - Clips")]
    [SerializeField] private AudioClip[] defaultFootsteps;
    [SerializeField] private AudioClip[] metalFootsteps;
    [SerializeField] private AudioClip[] woodFootsteps;

    [Header("Voice")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;

    [Header("Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource abilitySource;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundRayLength = 0.3f;

    private PlayerController _player;
    private float _distanceTraveled;
    private Vector3 _lastPosition;
    private bool _wasGrounded = true;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _lastPosition = transform.position;
    }

    private void Update()
    {
        bool grounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            out RaycastHit hit,
            groundRayLength + 0.1f,
            groundMask);

        HandleLanding(grounded);
        HandleFootsteps(grounded, hit);

        _wasGrounded = grounded;
    }

    private void HandleFootsteps(bool grounded, RaycastHit hit)
    {
        Vector3 delta = transform.position - _lastPosition;
        delta.y = 0f;
        _lastPosition = transform.position;

        if (!grounded)
        {
            _distanceTraveled = 0f;
            return;
        }

        if (delta.magnitude < 0.001f) return;

        _distanceTraveled += delta.magnitude;

        float threshold = _player.IsCrouching ? crouchStepDistance
                        : _player.IsSprinting ? sprintStepDistance
                        : walkStepDistance;

        if (_distanceTraveled >= threshold)
        {
            _distanceTraveled = 0f;
            PlayFootstep(hit.collider);
        }
    }

    private void HandleLanding(bool grounded)
    {
        if (grounded && !_wasGrounded)
            PlayLand();
    }

    private void PlayFootstep(Collider ground)
    {
        AudioClip[] clips = ground.CompareTag("Metal") ? metalFootsteps
                          : ground.CompareTag("Wood")  ? woodFootsteps
                          : defaultFootsteps;

        if (clips == null || clips.Length == 0) return;
        footstepSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    public void PlayJump()
    {
        if (jumpClip != null)
            voiceSource.PlayOneShot(jumpClip);
    }

    public void PlayLand()
    {
        if (landClip != null)
            voiceSource.PlayOneShot(landClip);
    }

    public void PlayAbility(AudioClip clip)
    {
        if (clip != null)
            abilitySource.PlayOneShot(clip);
    }
}
