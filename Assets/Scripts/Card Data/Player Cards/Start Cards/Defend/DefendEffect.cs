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
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 6(9) 얻습니다.
        int block = card.upgraded ? 9 : 6;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}