using System;
using UnityEngine;

public class PlayerHealth_UItest : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    public event Action<float, float> onHealthChanged; // (cur, max)

    void Awake()
    {
        currentHP = maxHP;
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(float dmg)
    {
        if (dmg <= 0f || currentHP <= 0f) return;
        currentHP = Mathf.Max(0f, currentHP - dmg);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHP <= 0f) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }
}
