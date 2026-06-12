using Fusion;
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField, ReadOnly] private float currentHealth;
    [SerializeField, ReadOnly] private float extraHealth;

    public float Current => currentHealth;
    public float Max => maxHealth;
    public float Extra => extraHealth;
    public float Total => maxHealth + extraHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        Init();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        float extraDamage = Mathf.Min(amount, extraHealth);
        extraHealth -= extraDamage;
        float remaining = amount - extraDamage;
        currentHealth = Mathf.Max(0f, currentHealth - remaining);
        OnHealthChanged();

        if (currentHealth == 0f)
            EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent());
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged();
    }

    public void SetExtraHealth(float amount)
    {
        extraHealth = amount;

        OnHealthChanged();
    }

    private void OnHealthChanged()
    {
        EventBus<HealthChangedEvent>.Raise(new HealthChangedEvent(this, currentHealth, extraHealth, maxHealth));
    }

    public void Init()
    {
        currentHealth = maxHealth;
        extraHealth = 0f;

        OnHealthChanged();
    }
}
