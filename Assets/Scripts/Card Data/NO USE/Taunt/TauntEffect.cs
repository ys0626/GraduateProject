public class TauntEffect : ICardEffect
{
    private CardInstance card;

    public TauntEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //업그레이드에 따른 수치
        int block = card.upgraded ? 8 : 7;
        int vulnerableTurns = card.upgraded ? 2 : 1;

        //Gain 7(8) Block
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);

        //Apply 1(2) Vulnerable
        cardTarget.AddStatus(new VulnerableStatus(vulnerableTurns));
    }
}