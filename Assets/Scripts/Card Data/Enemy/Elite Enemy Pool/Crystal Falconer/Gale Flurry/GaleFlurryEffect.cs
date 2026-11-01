public class GaleFlurryEffect : ICardEffect
{
    private CardInstance card;

    public GaleFlurryEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 5만큼 4번 줍니다.
        int damage;
        int times = 4;
        for (int i = 0; i < times; i++)
        {
            damage = 5;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}