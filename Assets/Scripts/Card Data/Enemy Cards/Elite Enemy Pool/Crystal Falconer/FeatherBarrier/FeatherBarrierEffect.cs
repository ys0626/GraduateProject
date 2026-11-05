public class FeatherBarrierEffect : ICardEffect
{
    private CardInstance card;

    public FeatherBarrierEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 7만큼 3번 얻습니다.
        int block;
        int times = 3;
        for (int i = 0; i < times; i++)
        {
            block = 7;
            block = cardUser.CalculateBlock(block);
            cardUser.GetBlock(block);
        }
    }
}