public class DustCloudEffect : ICardEffect
{
    private CardInstance card;

    public DustCloudEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //취약을 1 부여합니다.
        int vulnerableTurns = 1;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}