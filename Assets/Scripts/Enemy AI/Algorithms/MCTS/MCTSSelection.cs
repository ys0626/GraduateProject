/// <summary>
/// MCTS의 1. Selection 단계
/// </summary>
public static class MCTSSelection
{
    /// <summary>
    /// Expansion할 노드를 선택
    /// </summary>
    public static MCTSNode Select(
        MCTSNode node)
    {
        // =================================================
        // terminal이 아니고
        // fully expanded 상태면
        // 계속 UCT child로 내려감
        // =================================================

        while (
            !node.IsTerminal() &&
            node.IsFullyExpanded() &&
            node.children.Count > 0)
        {
            node = node.GetBestUCTChild();
        }

        return node;
    }
}