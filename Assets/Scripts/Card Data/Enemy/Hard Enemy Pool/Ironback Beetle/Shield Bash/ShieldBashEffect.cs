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
        //피해를 7 줍니다.
        int damage = 7;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //방어도를 7 얻습니다.
        int block = 7;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}