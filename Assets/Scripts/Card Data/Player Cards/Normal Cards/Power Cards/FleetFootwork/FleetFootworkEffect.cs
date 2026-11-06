public class FleetFootworkEffect : ICardEffect
{
    private CardInstance card;

    public FleetFootworkEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //민첩을 2(3) 얻습니다.
        int dexterity = card.upgraded ? 3 : 2;
        cardUser.AddStatus(new DexterityStatus(dexterity));
    }
}