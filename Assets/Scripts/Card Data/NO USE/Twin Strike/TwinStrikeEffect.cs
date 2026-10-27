public class TwinStrikeEffect : ICardEffect
{
    private CardInstance card;

    public TwinStrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int damage = card.upgraded ? 7 : 5;
        int hitTimes = 2;

        //twice
        for (int i = 0; i < hitTimes; i++) 
        {
            //Deal 5(7) Damage
            damage = card.upgraded ? 7 : 5;
            damage = cardUser.CalculateDamage(damage);
            cardTarget.TakeDamage(damage);
        }
    }
}