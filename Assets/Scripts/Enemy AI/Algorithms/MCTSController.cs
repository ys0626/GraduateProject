public class MCTSController : IEntityController
{
    // =====================================================
    // 구현해야 할 MCTS 함수
    // null을 리턴할 시 종료
    // 현재는 그냥 테스트용으로 손패의 왼쪽부터 사용 가능한
    //  카드 쭈루룩 선택해서 사용하는 방식임
    // =====================================================
    public CardInstance SelectCard(Entity enemy)
    {
        foreach (CardInstance card in enemy.hand)
        {
            if (card.currentCost <= enemy.CurrentEnergy)
            {
                return card;
            }
        }

        return null;
    }


}