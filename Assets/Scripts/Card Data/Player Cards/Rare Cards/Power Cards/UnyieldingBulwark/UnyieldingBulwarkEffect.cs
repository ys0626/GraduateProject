public class UnyieldingBulwarkEffect : ICardEffect
{
    private CardInstance card;

    public UnyieldingBulwarkEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //더 이상 턴 시작 시 방어도를 잃지 않습니다.
        cardUser.AddStatus(new BlockRetentionStatus());
    }
}