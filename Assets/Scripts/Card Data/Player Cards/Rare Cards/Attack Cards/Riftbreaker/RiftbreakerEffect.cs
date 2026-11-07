public class RiftbreakerEffect : ICardEffect
{
    private CardInstance card;

    public RiftbreakerEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //피해를 14(18) 줍니다.
        int damage = card.upgraded ? 18 : 14;
        damage = cardUser.CalculateDamage(damage);
        cardTarget.TakeDamage(damage);

        //취약을 3(4) 부여합니다.
        int vulnerableTurns = card.upgraded ? 4 : 3;
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}