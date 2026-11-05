public class RendEffect : ICardEffect
{
    private CardInstance card;

    public RendEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 2만큼 4(5)번 줍니다.
        int damage;
        int times = card.upgraded ? 5 : 4;
        for (int i = 0; i < times; i++)
        {
            damage = 2;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}