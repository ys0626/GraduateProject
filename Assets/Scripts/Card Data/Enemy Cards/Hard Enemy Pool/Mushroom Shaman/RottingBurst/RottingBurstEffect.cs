public class RottingBurstEffect : ICardEffect
{
    private CardInstance card;

    public RottingBurstEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 7 줍니다.
        int damage = 7;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //약화를 1 부여합니다.
        int weakTurns = 1;
        cardTarget.AddStatus(new WeakStatus(weakTurns));
    }
}