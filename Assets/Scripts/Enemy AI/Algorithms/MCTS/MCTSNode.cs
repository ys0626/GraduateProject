using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MCTS 탐색 트리의 노드
/// </summary>
public class MCTSNode
{
    // =====================================================
    // 현재 게임 상태
    // =====================================================

    public SimGameState state;

    // =====================================================
    // 트리 구조
    // =====================================================

    public MCTSNode parent;
    public List<MCTSNode> children = new List<MCTSNode>();

    // =====================================================
    // MCTS 통계
    // =====================================================

    public int visitCount;
    public float totalReward;

    // =====================================================
    // Action space
    // =====================================================

    public List<MCTSAction> untriedActions;
    public MCTSAction actionFromParent;

    public TurnPlayHistory turnHistory;

    // =====================================================
    // 생성자
    // =====================================================

    public MCTSNode(
        SimGameState state,
        MCTSNode parent = null,
        MCTSAction actionFromParent = default)
    {
        this.state = state;
        this.parent = parent;
        this.actionFromParent = actionFromParent;

        turnHistory = parent != null
            ? parent.turnHistory.Extend(actionFromParent.cardKey.data)
            : default;

        InitializeUntriedActions();
    }

    // =====================================================
    // action 초기화
    // =====================================================

    private void InitializeUntriedActions()
    {
        untriedActions = MCTSActionGenerator.GetLegalActions(state, turnHistory);
    }

    // =====================================================
    // Fully Expanded 여부
    // =====================================================

    public bool IsFullyExpanded()
    {
        return untriedActions == null || untriedActions.Count == 0;
    }

    // =====================================================
    // Terminal 여부
    // =====================================================

    public bool IsTerminal()
    {
        return state.self.CurrentHP <= 0 ||
               state.opponent.CurrentHP <= 0;
    }

    // =====================================================
    // UCT 선택
    // =====================================================

    public MCTSNode GetBestUCTChild(float explorationConstant = 1f)
    {
        if (children == null || children.Count == 0)
            return null;

        List<MCTSNode> unvisited =
            children.FindAll(c => c.visitCount == 0);

        if (unvisited.Count > 0)
        {
            return unvisited[
                Random.Range(0, unvisited.Count)];
        }

        MCTSNode bestChild = null;
        float bestUCT = float.MinValue;

        foreach (MCTSNode child in children)
        {
            float exploitation =
                child.totalReward / child.visitCount;

            float exploration =
                explorationConstant *
                Mathf.Sqrt(
                    Mathf.Log(Mathf.Max(1, visitCount)) /
                    child.visitCount
                );

            float uct = exploitation + exploration;

            if (uct > bestUCT)
            {
                bestUCT = uct;
                bestChild = child;
            }
        }

        return bestChild;
    }

    // =====================================================
    // 가장 많이 방문된 child
    // =====================================================

    public MCTSNode GetMostVisitedChild()
    {
        MCTSNode bestChild = null;
        int bestVisit = -1;

        foreach (MCTSNode child in children)
        {
            if (child.visitCount > bestVisit)
            {
                bestVisit = child.visitCount;
                bestChild = child;
            }
        }

        return bestChild;
    }

    //평균 보상값 계산.
    public float AverageReward =>
        visitCount > 0 ? totalReward / visitCount : 0f;

    //30번(중심극한정리 상 1000번 반복할 때 적당(클로드 피셜)) 이상 방문한 경우 시뮬레이션을 건너뛰도록 설정
    public bool ShouldSkipSimulation(int visitThreshold = 30)
    {
        return visitCount >= visitThreshold;
    }
}