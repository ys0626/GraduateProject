public class FuriousStrikeEffect : ICardEffect
{
    private CardInstance card;

    public FuriousStrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 8(12) 줍니다.
        int damage = card.upgraded ? 12 : 8;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}