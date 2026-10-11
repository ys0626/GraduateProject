using UnityEngine;

/// <summary>
/// MCTS 알고리즘
/// </summary>
public class MCTSController : IEntityController
{
    // =====================================================
    // Tree Reuse
    // =====================================================

    // 직전 탐색에서 고른 child 노드, 같은 턴의 다음 탐색에서 루트로 재사용을 시도
    // Controller가 전투마다 새로 생성되므로 전투가 바뀌면 자동으로 초기화됨
    private MCTSNode reusableNode;

    public CardInstance SelectCard(Entity entity)
    {
        // =================================================
        // 1. MCTS로 Action 선택
        // =================================================

        MCTSNode chosenNode;

        MCTSAction action = MCTSSearch.Search(entity, reusableNode, out chosenNode);

        // 다음 탐색에서 재사용할 노드 갱신 (Tree Reuse off, 턴 종료, Lethal Check 사용 시 null)
        reusableNode = chosenNode;

        // =================================================
        // 2. 실제 hand에서 CardInstance 찾기
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

        return null;
    }
}