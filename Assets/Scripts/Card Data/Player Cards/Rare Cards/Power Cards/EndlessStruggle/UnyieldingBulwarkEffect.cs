public class EndlessStruggleEffect : ICardEffect
{
    private CardInstance card;

    public EndlessStruggleEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //매 턴 시작 시 힘을 2(3) 얻습니다.
        int strengthPerTurn = card.upgraded ? 3 : 2;
        cardUser.AddStatus(new StrengthPerTurnStatus(strengthPerTurn));
    }
}