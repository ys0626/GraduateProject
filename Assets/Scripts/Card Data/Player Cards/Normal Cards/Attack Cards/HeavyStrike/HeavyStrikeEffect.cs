public class HeavyStrikeEffect : ICardEffect
{
    private CardInstance card;

    public HeavyStrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 8(11) 줍니다.
        int damage = card.upgraded ? 11 : 8;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}