public class SpinningShellEffect : ICardEffect
{
    private CardInstance card;

    public SpinningShellEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 12 줍니다.
        int damage = 12;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //방어도를 10 얻습니다.
        int block = 10;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}