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
        //약화를 1 부여합니다.
        int weakTurns = 1;
        cardTarget.AddStatus(new WeakStatus(weakTurns));
    }
}