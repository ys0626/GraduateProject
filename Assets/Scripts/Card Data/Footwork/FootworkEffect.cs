using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FootworkEffect : ICardEffect
{
    private CardInstance card;

    public FootworkEffect(CardInstance card)
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
        int dexterity = card.upgraded ? 3 : 2;

        //Gain 2 Dexterity
        cardTarget.AddStatus(new DexterityStatus(dexterity));
    }
}