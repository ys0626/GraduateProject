public class IronResolveEffect : ICardEffect
{
    private CardInstance card;

    public IronResolveEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //힘을 4 얻습니다.
        int strength = 4;
        cardUser.AddStatus(new StrengthStatus(strength));
    }
}