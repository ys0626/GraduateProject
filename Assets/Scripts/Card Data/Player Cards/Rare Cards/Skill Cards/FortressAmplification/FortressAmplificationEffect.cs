public class FortressAmplificationEffect : ICardEffect
{
    private CardInstance card;

    public FortressAmplificationEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도가 2배로 증가합니다.
        cardUser.Block *= 2;
    }
}