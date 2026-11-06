public class CombatManualEffect : ICardEffect
{
    private CardInstance card;

    public CombatManualEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //힘을 1(2) 얻습니다.
        int strength = card.upgraded ? 2 : 1;
        cardUser.AddStatus(new StrengthStatus(strength));

        //민첩을 1(2) 얻습니다.
        int dexterity = card.upgraded ? 2 : 1;
        cardUser.AddStatus(new DexterityStatus(dexterity));
    }
}