/// <summary>
/// MCTS의 1. Selection 단계
/// </summary>
public static class MCTSSelection
{
    /// <summary>
    /// Expansion할 노드를 선택
    /// </summary>
    public static MCTSNode Select(MCTSNode node)
    {
        if (node == null) return null;

        // 평가 방식의 보상 범위에 맞춘 탐험 상수 (기존 평가 1, 개선 평가 2√2)
        float explorationConstant =
            MCTSSimulation.GetExplorationConstant();

        while (node != null &&
               !node.IsTerminal() &&
               node.IsFullyExpanded() &&
               node.children != null &&
               node.children.Count > 0)
        {
            MCTSNode best = node.GetBestUCTChild(explorationConstant);
            if (best == null) break;

            node = best;
        }

        return node;
    }
}