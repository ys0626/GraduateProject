using UnityEngine;

/// <summary>
/// MCTS 전체 탐색 수행
/// </summary>
public static class MCTSSearch
{
    // =====================================================
    // 설정값
    // =====================================================

    private const int ITERATIONS = 1000;

    // =====================================================
    // Search
    // =====================================================

    /// <summary>
    /// 현재 상태에서 가장 좋은 action 선택
    /// </summary>
    public static MCTSAction Search(Entity entity)
    {
        // =================================================
        // 사용 가능한 카드 확인
        // =================================================

        bool hasPlayableCard = false;

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
            {
                hasPlayableCard = true;
                break;
            }
        }

        // 낼 카드 없음 → 턴 종료
        if (!hasPlayableCard)
        {
            return default;
        }

        // =================================================
        // 루트 상태 생성
        // =================================================

        SimGameState rootState =
            SimGameState.Create(entity);

        MCTSNode root =
            new MCTSNode(rootState);

        // =================================================
        // MCTS 반복
        // =================================================

        for (int i = 0; i < ITERATIONS; i++)
        {
            // 1. Selection
            MCTSNode selected =
                MCTSSelection.Select(root);

            if (selected == null || selected.state == null)
                continue;

            // 2. Expansion
            MCTSNode expanded =
                MCTSExpansion.Expand(selected);

            // 3. Simulation
            MCTSNode simulationNode =
                expanded ?? selected;

            if (simulationNode == null || simulationNode.state == null)
                continue;

            float reward =
                MCTSSimulation.Simulate(simulationNode.state);

            // 4. Backpropagation
            MCTSBackpropagation.Backpropagate(
                simulationNode,
                reward
            );
        }

        // =================================================
        // 최종 선택
        // =================================================

        Debug.Log("===== ROOT CHILDREN STATS =====");

        foreach (MCTSNode child in root.children)
        {
            float avg = child.visitCount > 0
                ? child.totalReward / child.visitCount
                : 0f;

            Debug.Log(
                $"Action: {child.actionFromParent.cardKey.data?.name ?? "END_TURN"} | " +
                $"Visit: {child.visitCount} | " +
                $"Total: {child.totalReward} | " +
                $"Avg: {avg}"
            );
        }

        MCTSNode bestChild =
            root.GetMostVisitedChild();

        if (bestChild == null)
        {
            return default;
        }

        // =================================================
        // Action 반환
        // =================================================

        return bestChild.actionFromParent;
    }
}