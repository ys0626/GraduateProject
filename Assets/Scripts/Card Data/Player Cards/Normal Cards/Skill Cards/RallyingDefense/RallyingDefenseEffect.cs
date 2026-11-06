public class RallyingDefenseEffect : ICardEffect
{
    private CardInstance card;

    public RallyingDefenseEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 6(9) 얻습니다.
        int block = card.upgraded ? 9 : 6;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);

        //약화를 1(2) 부여합니다.
        int weakTurns = card.upgraded ? 2 : 1;
        cardTarget.AddStatus(new WeakStatus(weakTurns));
    }
}