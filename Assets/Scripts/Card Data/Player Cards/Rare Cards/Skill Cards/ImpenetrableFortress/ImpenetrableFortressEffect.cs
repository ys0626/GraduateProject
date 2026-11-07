public class ImpenetrableFortressEffect : ICardEffect
{
    private CardInstance card;

    public ImpenetrableFortressEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 32 얻습니다.
        int block = 32;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}