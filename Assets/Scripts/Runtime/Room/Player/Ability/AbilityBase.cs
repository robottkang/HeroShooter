using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [SerializeField] private float cooldown = 5f;

    private float _cooldownTimer;

    public bool IsReady => _cooldownTimer <= 0f;
    public float Cooldown => cooldown;

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public bool TryUse(PlayerController player)
    {
        if (!IsReady) return false;
        UseAbility(player);
        _cooldownTimer = cooldown;
        return true;
    }

    protected abstract void UseAbility(PlayerController player);
}
