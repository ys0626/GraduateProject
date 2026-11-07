public class PerfectArmamentEffect : ICardEffect
{
    private CardInstance card;

    public PerfectArmamentEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //힘을 4(5) 얻습니다.
        int strength = card.upgraded ? 5 : 4;
        cardUser.AddStatus(new StrengthStatus(strength));

        //민첩을 4(5) 얻습니다.
        int dexterity = card.upgraded ? 5 : 4;
        cardUser.AddStatus(new DexterityStatus(dexterity));
    }
}