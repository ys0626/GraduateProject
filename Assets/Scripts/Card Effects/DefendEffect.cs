using UnityEngine;

public class DefendEffect : ICardEffect
{
    private CardInstance card;

    public DefendEffect(CardInstance card)
    {
        this.card = card;
    }

    public void Execute(GameState state)
    {
        Entity attacker = state.isPlayerTurn ? state.player : state.enemy;

        int block = card.data.blockAmount;

        // Weak(약화) 상태면 방어도 25% 감소
        if (attacker.statusEffects.TryGetValue(EffectType.Weak, out int weak) && weak > 0)
            block = Mathf.FloorToInt(block * 0.75f);

        block = Mathf.Max(0, block);
        attacker.block += block;

        Debug.Log($"[Defend] {block} 블록 획득");
    }
}
