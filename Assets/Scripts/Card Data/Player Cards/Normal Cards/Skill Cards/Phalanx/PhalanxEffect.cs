public class PhalanxEffect : ICardEffect
{
    private CardInstance card;

    public PhalanxEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 2만큼 3(4)번 얻습니다.
        int block;
        int times = card.upgraded ? 4 : 3;
        for (int i = 0; i < times; i++)
        {
            block = 2;
            block = cardUser.CalculateBlock(block);
            cardUser.GetBlock(block);
        }
    }
}