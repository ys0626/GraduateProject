public class SoulHarvestEffect : ICardEffect
{
    private CardInstance card;

    public SoulHarvestEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //적이 이미 죽어 있다면 실행하지 않음
        if (cardUser == null || cardTarget == null || cardTarget.CurrentHP <= 0)
            return;

        //피해를 12(15) 줍니다.
        int damage = card.upgraded ? 15 : 12;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //이 카드로 적을 처치하면 최대 체력이 7(10) 증가합니다.
        if (cardTarget.CurrentHP <= 0)
        {
            cardUser.MaxHP += card.upgraded ? 10 : 7;
            cardUser.CurrentHP += card.upgraded ? 10 : 7;
        }
    }
}