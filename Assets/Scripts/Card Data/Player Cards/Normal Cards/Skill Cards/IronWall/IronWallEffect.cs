public class IronWallEffect : ICardEffect
{
    private CardInstance card;

    public IronWallEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 8(11) 얻습니다.
        int block = card.upgraded ? 11 : 8;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}