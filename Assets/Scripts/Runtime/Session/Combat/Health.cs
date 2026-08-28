using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    [Networked, OnChangedRender(nameof(OnHealthChangedRender))]
    private float CurrentHealth { get; set; }

    [Networked, OnChangedRender(nameof(OnHealthChangedRender))]
    private float ExtraHealth { get; set; }

    public float Current => CurrentHealth;
    public float Max => maxHealth;
    public float Extra => ExtraHealth;
    public float Total => maxHealth + ExtraHealth;
    public bool IsDead => CurrentHealth <= 0f;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            Init();
    }

    public void TakeDamage(float amount)
    {
        RPC_TakeDamage(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_TakeDamage(float amount)
    {
        if (IsDead) return;

        float extraDamage = Mathf.Min(amount, ExtraHealth);
        ExtraHealth -= extraDamage;
        float remaining = amount - extraDamage;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - remaining);

        if (CurrentHealth == 0f)
            EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent(Object.InputAuthority));
    }

    public void Heal(float amount)
    {
        RPC_Heal(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public void SetExtraHealth(float amount)
    {
        RPC_SetExtraHealth(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetExtraHealth(float amount)
    {
        ExtraHealth = amount;
    }

    public void Init()
    {
        CurrentHealth = maxHealth;
        ExtraHealth = 0f;
    }

    private void OnHealthChangedRender()
    {
        EventBus<HealthChangedEvent>.Raise(new HealthChangedEvent(Object.InputAuthority, CurrentHealth, ExtraHealth, maxHealth));
    }
}
