public class DragonBarrageEffect : ICardEffect
{
    private CardInstance card;

    public DragonBarrageEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 2만큼 5번 줍니다.
        int damage;
        int times = 5;
        for (int i = 0; i < times; i++)
        {
            damage = 2;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}