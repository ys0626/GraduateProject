using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InflameEffect : ICardEffect
{
    private CardInstance card;

    public InflameEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int strength = card.upgraded ? 3 : 2;

        //Gain 2 Strength
        cardTarget.AddStatus(new StrengthStatus(strength));
    }
}