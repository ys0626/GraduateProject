public class ShatterEffect : ICardEffect
{
    private CardInstance card;

    public ShatterEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 10(13) 줍니다.
        int damage = card.upgraded ? 13 : 10;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //약화를 2(3) 부여합니다.
        int weakTurns = card.upgraded ? 3 : 2;
        cardTarget.AddStatus(new WeakStatus(weakTurns));
    }
}