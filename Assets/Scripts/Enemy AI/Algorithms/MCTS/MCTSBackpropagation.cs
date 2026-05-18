using UnityEngine;

/// <summary>
/// MCTS의 4. Backpropagation 단계
/// </summary>
public static class MCTSBackpropagation
{
    /// <summary>
    /// Simulation 결과를 root까지 전파
    /// </summary>
    public static void Backpropagate(
        MCTSNode node,
        float reward)
    {
        MCTSNode current = node;

        while (current != null)
        {
            current.visitCount++;
            current.totalReward += reward;

            current = current.parent;
        }
    }
}