public class BlazingChargeEffect : ICardEffect
{
    private CardInstance card;

    public BlazingChargeEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 12 줍니다.
        int damage = 12;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //취약을 1 부여합니다.
        int vulnerableTurns = 1;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}