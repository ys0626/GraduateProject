public class ApocalypticRoarEffect : ICardEffect
{
    private CardInstance card;

    public ApocalypticRoarEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 12만큼 2번 줍니다.
        int damage;
        int times = 2;
        for (int i = 0; i < times; i++)
        {
            damage = 12;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}