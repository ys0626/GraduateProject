public class PiercingThrustEffect : ICardEffect
{
    private CardInstance card;

    public PiercingThrustEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 5(7) 줍니다.
        int damage = card.upgraded ? 7 : 5;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //취약을 1(2) 부여합니다.
        int vulnerableTurns = card.upgraded ? 2 : 1;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}