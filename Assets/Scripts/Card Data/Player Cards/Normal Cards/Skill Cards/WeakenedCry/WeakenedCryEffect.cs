public class WeakenedCryEffect : ICardEffect
{
    private CardInstance card;

    public WeakenedCryEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //약화를 2(3) 부여합니다.
        int weakTurns = card.upgraded ? 3 : 2;
        cardTarget.AddStatus(new WeakStatus(weakTurns));
    }
}