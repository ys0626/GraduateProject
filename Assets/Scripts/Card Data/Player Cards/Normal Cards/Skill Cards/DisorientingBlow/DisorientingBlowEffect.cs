public class DisorientingBlowEffect : ICardEffect
{
    private CardInstance card;

    public DisorientingBlowEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //약화를 1(2) 부여합니다.
        int weakTurns = card.upgraded ? 2 : 1;
        cardTarget.AddStatus(new WeakStatus(weakTurns));

        //취약을 1(2) 부여합니다.
        int vulnerableTurns = card.upgraded ? 2 : 1;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}