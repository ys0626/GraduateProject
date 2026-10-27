public class EntrenchEffect : ICardEffect
{
    private CardInstance card;

    public EntrenchEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //Double your Block
        cardUser.Block *= 2;
    }
}