public class EmergencyBarrierEffect : ICardEffect
{
    private CardInstance card;

    public EmergencyBarrierEffect(CardInstance card)
    {
        this.card = card;
    }

    /// <summary>
    /// 카드 사용 시의 효과
    /// </summary>
    public void Execute(IBattleEntity cardUser, IBattleEntity cardTarget)
    {
        //방어도를 4(7) 얻습니다.
        int block = card.upgraded ? 7 : 4;
        block = cardUser.CalculateBlock(block);
        cardUser.GetBlock(block);
    }
}