using UnityEngine;
using static BattleManager;

public class StrikeEffect : ICardEffect
{
    private CardInstance card;

    public StrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int damage = card.upgraded ? 9 : 6;
        
        //Deal 6(9) Damage
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}