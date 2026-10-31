public class CrushingPincerEffect : ICardEffect
{
    private CardInstance card;

    public CrushingPincerEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 8 줍니다.
        int damage = 8;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //취약을 1 부여합니다.
        int vulnerableTurns = 1;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}