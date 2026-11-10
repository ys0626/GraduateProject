
using UnityEngine;

/// <summary>
/// MCTS 알고리즘
/// </summary>
public class MCTSController : IEntityController
{
    // =====================================================
    // Tree Reuse
    // =====================================================

    // 직전 탐색에서 고른 child 노드
    // 같은 턴의 다음 탐색에서 루트로 재사용을 시도
    // Controller가 전투마다 새로 생성되므로 전투가 바뀌면 자동으로 초기화됨
    private MCTSNode reusableNode;

    public CardInstance SelectCard(Entity entity)
    {
        // =================================================
        // 1. MCTS로 Action 선택
        // =================================================

        MCTSNode chosenNode = null;
        MCTSAction action = default;

        // 실제 게임의 전역 난수 상태 저장
        Random.State previousState = Random.state;

        try
        {
            action = MCTSSearch.Search(
                entity,
                reusableNode,
                out chosenNode
            );
        }
        finally
        {
            // MCTS 탐색에서 사용한 난수를 실제 게임에 반영하지 않음
            Random.state = previousState;
        }

        // 다음 탐색에서 재사용할 노드 갱신
        reusableNode = chosenNode;

        // =================================================
        // 2. 유효한 Action인지 확인
        // =================================================

        if (action.cardKey.data == null)
        {
            reusableNode = null;
            return null;
        }

        // =================================================
        // 3. 실제 hand에서 CardInstance 찾기
        // =================================================

        foreach (CardInstance card in entity.hand)
        {
            if (card.data == action.cardKey.data &&
                card.currentCost == action.cardKey.cost &&
                card.upgraded == action.cardKey.upgraded &&
                card.exhaust == action.cardKey.exhaust &&
                card.ethereal == action.cardKey.ethereal)
            {
                return card;
            }
        }

        // 실제 손패에서 일치하는 카드를 찾지 못한 경우
        // 기존 Tree Reuse 데이터 폐기
        reusableNode = null;

        return null;
    }
}
