public class BashEffect : ICardEffect
{
    private CardInstance card;

    public BashEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int damage = card.upgraded ? 10 : 8;
        int vulnerableTurns = card.upgraded ? 3 : 2;

        //Deal 8(10) damage
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //Apply 2(3) Vulnerable
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}