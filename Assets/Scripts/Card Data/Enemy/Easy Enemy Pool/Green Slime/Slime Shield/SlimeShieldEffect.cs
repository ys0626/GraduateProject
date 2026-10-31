public class SlimeShieldEffect : ICardEffect
{
    private CardInstance card;

    public SlimeShieldEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 7 얻습니다.
        int block = 7;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}