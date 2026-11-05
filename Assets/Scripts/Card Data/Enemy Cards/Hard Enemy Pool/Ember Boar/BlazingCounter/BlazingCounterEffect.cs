public class BlazingCounterEffect : ICardEffect
{
    private CardInstance card;

    public BlazingCounterEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 6 줍니다.
        int damage = 6;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //방어도를 7 얻습니다.
        int block = 7;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}