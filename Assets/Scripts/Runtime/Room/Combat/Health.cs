using Fusion;
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField, ReadOnly] private float current;

    public float Current => current;
    public float Max => maxHealth;
    public bool IsDead => current <= 0f;

    public delegate void OnHealthChangedHandler(float currentHealth, float maxHealth); 
    public event OnHealthChangedHandler OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        current = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        current = Mathf.Max(0f, current - amount);
        OnHealthChanged?.Invoke(current, maxHealth);

        if (current == 0f)
            OnDied?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        current = Mathf.Min(maxHealth, current + amount);
        OnHealthChanged?.Invoke(current, maxHealth);
    }
}
