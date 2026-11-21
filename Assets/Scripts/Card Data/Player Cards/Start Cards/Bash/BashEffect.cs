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
        //피해를 9(11) 줍니다.
        int damage = card.upgraded ? 11 : 9;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //취약을 2(3) 부여합니다.
        int vulnerableTurns = card.upgraded ? 3 : 2;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}