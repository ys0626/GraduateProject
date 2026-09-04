using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MCTS의 2. Expansion 단계
/// </summary>
public static class MCTSExpansion
{
    /// <summary>
    /// node를 1회 확장
    /// </summary>
    public static MCTSNode Expand(MCTSNode node)
    {
        // =================================================
        // 0. action 생성
        // =================================================

        if (node.untriedActions == null || node.untriedActions.Count == 0)
            return null;

        int idx = Random.Range(0, node.untriedActions.Count);
        MCTSAction action = node.untriedActions[idx];

        node.untriedActions.RemoveAt(idx);

        // =================================================
        // 1. 상태 복사
        // =================================================

        SimGameState newState = node.state.Clone();

        // =================================================
        // 2. Action 적용
        // =================================================

        ApplyAction(newState, action);

        // =================================================
        // 3. child 생성
        // =================================================

        MCTSNode child = new MCTSNode(
            newState,
            node,
            action
        );

        // =================================================
        // 4. parent-child 연결
        // =================================================

        node.children.Add(child);

        return child;
    }

    // =====================================================
    // Action 실행 (instance resolution 포함)
    // =====================================================

    internal static void ApplyAction(SimGameState state, MCTSAction action)
    {
        // =================================================
        // CardKey → 실제 CardInstance 후보 찾기
        // =================================================

        List<CardInstance> candidates = state.self.hand.FindAll(card =>
            card.data == action.cardKey.data &&
            card.currentCost == action.cardKey.cost &&
            card.upgraded == action.cardKey.upgraded &&
            card.exhaust == action.cardKey.exhaust &&
            card.ethereal == action.cardKey.ethereal
        );

        if (candidates.Count == 0)
            return;

        // =================================================
        // instance 선택 (stochastic resolution)
        // =================================================

        CardInstance selected =
            candidates[Random.Range(0, candidates.Count)];

        // =================================================
        // 카드 사용
        // =================================================

        SimBattleHelper.TryUseCard(
            state.self,
            state.opponent,
            selected
        );
    }
}