public class BodySlamEffect : ICardEffect
{
    private CardInstance card;

    public BodySlamEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int damage = cardUser.Block;

        //Deal damage equal to your Block
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);
    }
}