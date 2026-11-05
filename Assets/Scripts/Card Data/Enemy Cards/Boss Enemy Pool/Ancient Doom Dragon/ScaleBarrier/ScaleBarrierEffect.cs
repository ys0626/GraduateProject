public class ScaleBarrierEffect : ICardEffect
{
    private CardInstance card;

    public ScaleBarrierEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 8만큼 2번 얻습니다.
        int block;
        int times = 2;
        for (int i = 0; i < times; i++)
        {
            block = 8;
            block = cardUser.CalculateBlock(block);
            cardUser.GetBlock(block);
        }
    }
}