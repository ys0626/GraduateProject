using UnityEngine;

public class StrikeEffect : ICardEffect
{
    private CardInstance card;

    public StrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    public void Execute(GameState state)
    {
        Entity attacker = state.isPlayerTurn ? state.player : state.enemy;
        Entity defender = state.isPlayerTurn ? state.enemy : state.player;

        int damage = card.data.damage;

        // Strength(힘) 적용
        if (attacker.statusEffects.TryGetValue(EffectType.Strength, out int str))
            damage += str;

        // Weak(약화) 적용: 공격자가 약화 상태면 데미지 25% 감소
        if (attacker.statusEffects.TryGetValue(EffectType.Weak, out int weak) && weak > 0)
            damage = Mathf.FloorToInt(damage * 0.75f);

        // Vulnerable(취약) 적용: 피공격자가 취약 상태면 받는 데미지 50% 증가
        if (defender.statusEffects.TryGetValue(EffectType.Vulnerable, out int vuln) && vuln > 0)
            damage = Mathf.FloorToInt(damage * 1.5f);

        damage = Mathf.Max(0, damage);
        defender.TakeDamage(damage);

        Debug.Log($"[Strike] {damage} 데미지 (Str:{str} Weak:{weak} Vuln:{vuln})");
    }
}
