public class CorruptedKnight_ShieldBashEffect : ICardEffect
{
    private CardInstance card;

    public CorruptedKnight_ShieldBashEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //현재 방어도만큼의 피해를 줍니다.
        int damage = cardUser.Block;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}