using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Footstep - Distance")]
    [SerializeField] private float walkStepDistance = 0.6f;
    [SerializeField] private float sprintStepDistance = 0.4f;
    [SerializeField] private float crouchStepDistance = 0.8f;

    [Header("Clips")]
    [SerializeField] private AudioClip[] defaultFootstepClips;
    [SerializeField] private AudioClip[] metalFootstepClips;
    [SerializeField] private AudioClip[] woodFootstepClips;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;

    [Header("Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource abilitySource;

    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        _player.OnJump += PlayJump;
    }

    private void Update()
    {
        if (_player.CC.velocity.magnitude > 0.1f && _player.CC.isGrounded) { }
    }

    private void PlayFootstep(Collider ground)
    {
        AudioClip[] clips = ground.CompareTag("Metal") ? metalFootstepClips
                          : ground.CompareTag("Wood")  ? woodFootstepClips
                          : defaultFootstepClips;

        if (footstepSource.isPlaying) return;
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
