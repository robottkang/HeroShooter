using UnityEngine;
using Cysharp.Threading.Tasks;

[System.Flags]
public enum AbilityBlockFlags { None = 0, Move = 1, Action = 2 }

public abstract class AbilityBase : MonoBehaviour
{
    [SerializeField] protected float cooldown = 5f;
    [SerializeField] protected float abilityActiveDuration = 0.2f;

    protected float _cooldownTimer;

    public bool IsReady => _cooldownTimer <= 0f;
    public bool IsActive { get; private set; }
    public float Cooldown => cooldown;
    public abstract AbilityBlockFlags BlockFlags { get; }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public bool TryUse(PlayerController player)
    {
        if (!IsReady || IsActive) return false;
        _cooldownTimer = cooldown;
        UseAbilityAsync(player).Forget();
        TrackActiveDurationAsync(player).Forget();
        return true;
    }
    protected async UniTaskVoid TrackActiveDurationAsync(PlayerController player)
    {
        IsActive = true;
        await UniTask.WaitForSeconds(abilityActiveDuration,
            cancellationToken: player.destroyCancellationToken);
        IsActive = false;
    }

    protected abstract UniTask UseAbilityAsync(PlayerController player);
}
