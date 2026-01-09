using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public event Action<int, int> OnHealthChanged; // current health, max health
    public event Action OnDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void Initialize(CharacterClassSO characterClassSO)
    {
        maxHealth = characterClassSO.maxHealth;
        currentHealth = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    private void Die()
    {
        Debug.Log($"{gameObject.name} has died");
        OnDeath?.Invoke();
    }

}
