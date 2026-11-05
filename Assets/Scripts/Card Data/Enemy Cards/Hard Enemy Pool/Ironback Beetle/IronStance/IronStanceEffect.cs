public class IronStanceEffect : ICardEffect
{
    private CardInstance card;

    public IronStanceEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 12 얻습니다.
        int block = 12;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}