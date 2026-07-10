using UnityEngine;
using Cysharp.Threading.Tasks;

[System.Flags]
public enum AbilityBlockFlags { None = 0, Move = 1, Action = 2 }

public abstract class AbilityBase : MonoBehaviour
{
    [SerializeField] protected Sprite icon;
    [Header("Ability Settings")]
    [SerializeField] protected int maxCharges = 1;
    [SerializeField] protected float cooldown = 5f;
    [SerializeField] protected float abilityActiveDuration = 0.2f;

    protected int _charges;
    protected float _cooldownTimer;

    public bool IsReady => _cooldownTimer <= 0f && _charges > 0;
    public bool IsActive { get; private set; }
    public float Cooldown => cooldown;
    public abstract AbilityBlockFlags BlockFlags { get; }

    private void Awake()
    {
        _charges = maxCharges;
        EventBus<AbilityChangedEvent>.Raise(new AbilityChangedEvent(icon, _charges));
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public bool TryUse(PlayerController player)
    {
        if (!IsReady || IsActive) return false;
        _cooldownTimer = cooldown;
        _charges -= 1;
        UseAbilityAsync(player).Forget();
        ActivateForDurationAsync(player).Forget();
        EventBus<AbilityStateChangedEvent>.Raise(new AbilityStateChangedEvent(_charges));
        return true;
    }

    protected async UniTaskVoid ActivateForDurationAsync(PlayerController player)
    {
        IsActive = true;
        await UniTask.WaitForSeconds(abilityActiveDuration,
            cancellationToken: player.destroyCancellationToken);
        IsActive = false;
    }

    protected abstract UniTask UseAbilityAsync(PlayerController player);
}
