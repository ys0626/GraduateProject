public class QuickJabEffect : ICardEffect
{
    private CardInstance card;

    public QuickJabEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 4(7) 줍니다.
        int damage = card.upgraded ? 7 : 4;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}