public class LastStandEffect : ICardEffect
{
    private CardInstance card;

    public LastStandEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 14(18) 얻습니다.
        int block = card.upgraded ? 18 : 14;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}