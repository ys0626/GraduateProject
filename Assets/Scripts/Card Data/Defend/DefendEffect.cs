using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DefendEffect : ICardEffect
{
    private CardInstance card;

    public DefendEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute()
    {
        Entity cardTarget = BattleManager.instance.GetCardTarget(card);

        //업그레이드에 따른 수치
        int block = card.upgraded ? 8 : 5;

        //Gain 5(8) Block
        block = cardTarget.CalculateBlock(block);
        cardTarget.GetBlock(block);
    }
}