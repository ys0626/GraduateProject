public class CrushingAdvanceEffect : ICardEffect
{
    private CardInstance card;

    public CrushingAdvanceEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 12(15) 줍니다.
        int damage = card.upgraded ? 15 : 12;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //약화를 1(2) 부여합니다.
        int weakTurns = card.upgraded ? 2 : 1;
        cardTarget.AddStatus(new WeakStatus(weakTurns));
    }
}