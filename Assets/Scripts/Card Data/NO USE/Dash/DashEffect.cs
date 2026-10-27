public class DashEffect : ICardEffect
{
    private CardInstance card;

    public DashEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int block = card.upgraded ? 13 : 10;
        int damage = card.upgraded ? 13 : 10;

        //Gain 10(13) Block
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);

        //Deal 10(13) damage
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}