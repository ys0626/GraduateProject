using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Burn, //발화
    Vulnerable, //취약
    Weak, //약화
    Strength, //힘
    LoseHealth, //턴마다 체력 감소
}
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

        block = Mathf.Max(0, block);
        currentHP = Mathf.Max(0, currentHP);
    }

    public Entity Clone()
    {
        return new Entity
        {
            currentHP = this.currentHP,
            maxHP = this.maxHP,
            block = this.block,
            statusEffects = new Dictionary<EffectType, int>(this.statusEffects)
        };
    }
}