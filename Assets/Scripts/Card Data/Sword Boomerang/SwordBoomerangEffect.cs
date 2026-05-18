using UnityEditor.Rendering.Universal;
using UnityEngine;
using static BattleManager;

public class SwordBoomerangEffect : ICardEffect
{
    private CardInstance card;

    public SwordBoomerangEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int damage = 3;
        int hitTimes = card.upgraded ? 7 : 5;

        //5(7)times
        for (int i = 0; i < hitTimes; i++) 
        {
            //Deal 3 Damage
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
            damage = 3;
        }
    }
}