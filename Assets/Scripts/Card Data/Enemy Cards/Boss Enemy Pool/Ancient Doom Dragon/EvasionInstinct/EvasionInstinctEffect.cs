public class EvasionInstinctEffect : ICardEffect
{
    private CardInstance card;

    public EvasionInstinctEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //민첩을 2 얻습니다.
        int dexterity = 2;
        cardUser.AddStatus(new DexterityStatus(dexterity));
    }
}