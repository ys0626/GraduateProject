public class StormOfBladesEffect : ICardEffect
{
    private CardInstance card;

    public StormOfBladesEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 4만큼 5(6)번 줍니다.
        int damage;
        int times = card.upgraded ? 6 : 5;
        for (int i = 0; i < times; i++)
        {
            damage = 4;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}