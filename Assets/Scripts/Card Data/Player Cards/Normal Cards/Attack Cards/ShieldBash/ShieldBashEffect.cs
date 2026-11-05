public class ShieldBashEffect : ICardEffect
{
    private CardInstance card;

    public ShieldBashEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 6(8) 줍니다.
        int damage = card.upgraded ? 8 : 6;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //방어도를 4(6) 얻습니다.
        int block = card.upgraded ? 6 : 4;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}