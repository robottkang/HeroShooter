using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float _current;

    public float Current => _current;
    public float Max => maxHealth;
    public bool IsDead => _current <= 0f;

    public event Action<float, float> OnHealthChanged;  // current, max
    public event Action OnDied;

    private void Awake()
    {
        _current = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _current = Mathf.Max(0f, _current - amount);
        OnHealthChanged?.Invoke(_current, maxHealth);

        if (_current == 0f)
            OnDied?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        _current = Mathf.Min(maxHealth, _current + amount);
        OnHealthChanged?.Invoke(_current, maxHealth);
    }
}
