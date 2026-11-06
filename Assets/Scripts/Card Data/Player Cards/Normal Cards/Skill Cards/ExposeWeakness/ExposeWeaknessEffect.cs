public class ExposeWeaknessEffect : ICardEffect
{
    private CardInstance card;

    public ExposeWeaknessEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //취약을 2(3) 부여합니다.
        int vulnerableTurns = card.upgraded ? 3 : 2;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}