public class CounterattackEffect : ICardEffect
{
    private CardInstance card;

    public CounterattackEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 7(9) 줍니다.
        int damage = card.upgraded ? 9 : 7;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //자신이 방어도를 보유하고 있다면 피해를 6(8) 추가로 줍니다.
        if (cardUser.Block > 0)
        {
            damage = card.upgraded ? 8 : 6;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}