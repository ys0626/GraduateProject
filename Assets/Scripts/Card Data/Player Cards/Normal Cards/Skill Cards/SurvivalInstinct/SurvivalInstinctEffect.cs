public class SurvivalInstinctEffect : ICardEffect
{
    private CardInstance card;

    public SurvivalInstinctEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 8(12) 얻습니다.
        int block = card.upgraded ? 12 : 8;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}