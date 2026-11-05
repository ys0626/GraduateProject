public class StrikeEffect : ICardEffect
{
    private CardInstance card;

    public StrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 6(9) 줍니다.
        int damage = card.upgraded ? 9 : 6;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}