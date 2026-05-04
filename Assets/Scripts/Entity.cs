// Entity.cs
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public int currentHP;
    public int maxHP;
    public int block;
    public Dictionary<EffectType, int> statusEffects = new();

    public void TakeDamage(int amount)
    {
        int absorbed = Mathf.Min(block, amount);
        block -= absorbed;
        currentHP -= (amount - absorbed);
    }
}