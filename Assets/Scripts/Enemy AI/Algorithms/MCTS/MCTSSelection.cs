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

        while (node != null &&
               !node.IsTerminal() &&
               node.IsFullyExpanded() &&
               node.children != null &&
               node.children.Count > 0)
        {
            MCTSNode best = node.GetBestUCTChild();
            if (best == null) break;

            node = best;
        }

        return node;
    }
}