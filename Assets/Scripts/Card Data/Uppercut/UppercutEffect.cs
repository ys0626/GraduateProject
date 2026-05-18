using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UppercutEffect : ICardEffect
{
    private CardInstance card;

    public UppercutEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int damage = 13;
        int weakTurns = card.upgraded ? 2 : 1;
        int vulnerableTurns = card.upgraded ? 2 : 1;

        //Deal 13 damage
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //Apply 1(2) Weak
        cardTarget.AddStatus(new WeakStatus(weakTurns));

        //Apply 1(2) Vulnerable
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}