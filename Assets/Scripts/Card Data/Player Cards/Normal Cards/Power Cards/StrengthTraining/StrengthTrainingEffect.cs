public class StrengthTrainingEffect : ICardEffect
{
    private CardInstance card;

    public StrengthTrainingEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //힘을 2(3) 얻습니다.
        int strength = card.upgraded ? 3 : 2;
        cardUser.AddStatus(new StrengthStatus(strength));
    }
}