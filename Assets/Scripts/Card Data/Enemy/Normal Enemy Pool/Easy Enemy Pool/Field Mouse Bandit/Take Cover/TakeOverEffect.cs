public class TakeOverEffect : ICardEffect
{
    private CardInstance card;

    public TakeOverEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 5 얻습니다.
        int block = 5;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}