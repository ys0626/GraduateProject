using UnityEngine;

/// <summary>
/// MCTS 알고리즘
/// </summary>
public class MCTSController : IEntityController
{
    public CardInstance SelectCard(Entity entity)
    {
        // =================================================
        // 1. MCTS로 Action 선택
        // =================================================

        MCTSAction action = MCTSSearch.Search(entity);

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