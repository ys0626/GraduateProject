public class WavesOfDefenseEffect : ICardEffect
{
    private CardInstance card;

    public WavesOfDefenseEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 4만큼 5(6)번 얻습니다.
        int block;
        int times = card.upgraded ? 6 : 5;
        for (int i = 0; i < times; i++)
        {
            block = 4;
            block = cardUser.CalculateBlock(block);
            cardUser.GetBlock(block);
        }
    }
}