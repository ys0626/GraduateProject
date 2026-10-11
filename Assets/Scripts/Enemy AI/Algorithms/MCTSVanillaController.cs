using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

/// <summary>
/// 바닐라 MCTS 알고리즘
/// 동적 탐색, 휴리스틱 롤아웃, 조기 종료, 캐싱, Tree Reuse를 모두 사용하지 않는 기본 구조의 MCTS
/// 기법별 효과를 비교하기 위한 기준선이며, 기존 MCTS 클래스(MCTSSearch, MCTSNode 등)는 사용하지 않는다.
/// 탐색 방식 관련 플래그는 참조하지 않고, 반복 횟수만 SimulationManager의 고정 반복 횟수를 따른다.
/// </summary>
public class MCTSVanillaController : IEntityController
{
    // =====================================================
    // 설정값
    // =====================================================

    private const int FIXED_ITERATIONS = 1000; // SimulationManager가 없을 때 사용하는 반복 횟수, 있을 때는 SimulationManager의 고정 반복 횟수를 따름
    private const float EXPLORATION_CONSTANT = 1.41421356f; // UCT 탐험 상수 (√2, 보상 범위가 [0, 1]일 때의 표준값)
    private const int ROLLOUT_MAX_STEPS = 200; // 롤아웃 최대 진행 횟수 (카드 사용 + 턴 종료), 승패가 나지 않아도 롤아웃이 끝나도록 하는 안전 상한

    private const int MEMORY_SAMPLE_INTERVAL = 20; // 이 탐색 횟수마다 한 번씩만 강제 GC로 정확한 메모리 측정 (MCTSSearch와 같은 주기, 해당 탐색의 ElapsedMs는 왜곡되므로 시간 분석에서 제외할 것)

    // =====================================================
    // 노드
    // =====================================================

    /// <summary>
    /// 내 턴의 카드 사용 순서를 표현하는 탐색 트리 노드
    /// </summary>
    private class VanillaNode
    {
        public SimGameState state;
        public VanillaNode parent;
        public List<VanillaNode> children = new List<VanillaNode>();

        public List<int> untriedCardIDs = new List<int>(); // 아직 확장하지 않은 행동 (카드 instanceID)
        public int actionCardID; // 부모에서 이 노드로 오기 위해 사용한 카드의 instanceID

        public int visitCount;
        public float totalReward;
    }

    // 후보 카드 수집용 버퍼, 매번 새 리스트를 만들지 않고 재사용
    private readonly List<CardInstance> candidateBuffer = new List<CardInstance>();

    // =====================================================
    // 측정용
    // =====================================================

    private static int searchCallCount = 0; // 탐색 호출 횟수 누적, 메모리 샘플링 주기 판단용

    private int rolloutTerminalCount; // 이번 탐색에서 승패까지 도달한 롤아웃 수
    private int rolloutCapCount; // 이번 탐색에서 안전 상한에 걸려 HP 비교로 끝난 롤아웃 수

    // =====================================================
    // 외부 호출
    // =====================================================

    public CardInstance SelectCard(Entity entity)
    {
        // =================================================
        // 사용 가능한 카드 확인
        // =================================================

        int playableCardCount = 0;

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
            {
                playableCardCount++;
            }
        }

        // 낼 카드 없음 → 턴 종료
        if (playableCardCount == 0)
        {
            return null;
        }

        // =================================================
        // 루트 상태 생성
        // =================================================

        SimGameState rootState =
            SimGameState.Create(entity);

        int iterations =
            SimulationManager.instance != null
            ? SimulationManager.instance.FixedSimulationBudget
            : FIXED_ITERATIONS;

        // =================================================
        // 측정 시작 (MCTSSearch와 같은 구간을 측정)
        // =================================================

        searchCallCount++;

        bool isMemorySampleTurn =
            searchCallCount % MEMORY_SAMPLE_INTERVAL == 0;

        long memoryBefore =
            isMemorySampleTurn
            ? System.GC.GetTotalMemory(true)
            : System.GC.GetTotalMemory(false);

        int nodeCount = 0;

        rolloutTerminalCount = 0;
        rolloutCapCount = 0;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        VanillaNode root =
            CreateNode(rootState, null, -1);

        // =================================================
        // MCTS 반복
        // =================================================

        for (int i = 0; i < iterations; i++)
        {
            // 1. Selection
            VanillaNode selected =
                Select(root);

            // 2. Expansion
            VanillaNode expanded =
                Expand(selected);

            if (expanded != null)
                nodeCount++;

            // 3. Simulation
            VanillaNode simulationNode =
                expanded ?? selected;

            float reward =
                Simulate(simulationNode.state);

            // 4. Backpropagation
            Backpropagate(simulationNode, reward);
        }

        // =================================================
        // 측정 종료 및 기록
        // =================================================

        stopwatch.Stop();

        long memoryAfter =
            isMemorySampleTurn
            ? System.GC.GetTotalMemory(true)
            : System.GC.GetTotalMemory(false);

        // =================================================
        // 최다 방문 자식 선택
        // =================================================

        VanillaNode bestChild =
            GetMostVisitedChild(root);

        // =================================================
        // 실제 hand에서 CardInstance 찾기
        // =================================================

        CardInstance selectedCard = null;

        if (bestChild != null)
        {
            foreach (CardInstance card in entity.hand)
            {
                if (card.instanceID == bestChild.actionCardID)
                {
                    selectedCard = card;
                    break;
                }
            }
        }

        // 선택 카드를 로그에 남기기 위해 선택 후 기록 (시간 측정 구간 밖)
        LogSearchResult(
            entity,
            rootState,
            selectedCard,
            playableCardCount,
            iterations,
            iterations,
            stopwatch.Elapsed.TotalMilliseconds,
            nodeCount,
            memoryAfter - memoryBefore,
            isMemorySampleTurn);

        return selectedCard;
    }

    // =====================================================
    // 노드 생성
    // =====================================================

    private VanillaNode CreateNode(
        SimGameState state,
        VanillaNode parent,
        int actionCardID)
    {
        VanillaNode node = new VanillaNode
        {
            state = state,
            parent = parent,
            actionCardID = actionCardID
        };

        // 내 턴이고 승패가 나지 않았을 때만 확장 가능
        bool expandable =
            state.selfTurn &&
            state.self.CurrentHP > 0 &&
            state.opponent.CurrentHP > 0;

        if (expandable)
        {
            CollectActionCards(state.self, candidateBuffer);

            foreach (CardInstance card in candidateBuffer)
            {
                node.untriedCardIDs.Add(card.instanceID);
            }
        }

        return node;
    }

    // =====================================================
    // 1. Selection
    // =====================================================

    private VanillaNode Select(VanillaNode root)
    {
        VanillaNode node = root;

        // 모든 행동이 확장된 노드는 UCT로 자식을 따라 내려감
        while (node.untriedCardIDs.Count == 0 &&
               node.children.Count > 0)
        {
            node = GetBestUCTChild(node);
        }

        return node;
    }

    private VanillaNode GetBestUCTChild(VanillaNode node)
    {
        VanillaNode best = null;

        float bestScore =
            float.NegativeInfinity;

        float logParentVisit =
            Mathf.Log(node.visitCount);

        foreach (VanillaNode child in node.children)
        {
            // 방문한 적 없는 자식 우선
            if (child.visitCount == 0)
            {
                return child;
            }

            float exploitation =
                child.totalReward / child.visitCount;

            float exploration =
                EXPLORATION_CONSTANT *
                Mathf.Sqrt(logParentVisit / child.visitCount);

            float score =
                exploitation + exploration;

            if (score > bestScore)
            {
                bestScore = score;
                best = child;
            }
        }

        return best;
    }

    // =====================================================
    // 2. Expansion
    // =====================================================

    /// <summary>
    /// 아직 시도하지 않은 행동 중 무작위 1개를 확장
    /// 확장할 행동이 없으면 null
    /// </summary>
    private VanillaNode Expand(VanillaNode node)
    {
        while (node.untriedCardIDs.Count > 0)
        {
            int index =
                Random.Range(0, node.untriedCardIDs.Count);

            int cardID =
                node.untriedCardIDs[index];

            node.untriedCardIDs.RemoveAt(index);

            // 상태 복사
            SimGameState next =
                node.state.Clone();

            // clone 내부 카드 찾기
            CardInstance simCard =
                FindCardByID(next.self, cardID);

            if (simCard == null)
            {
                continue;
            }

            // 카드 사용
            bool used =
                SimBattleHelper.TryUseCard(
                    next.self,
                    next.opponent,
                    simCard);

            if (!used)
            {
                continue;
            }

            // 추가 행동 불가능하면 턴 종료
            if (!HasPlayableCard(next.self))
            {
                EndTurn(next);
            }

            VanillaNode child =
                CreateNode(next, node, cardID);

            node.children.Add(child);

            return child;
        }

        return null;
    }

    // =====================================================
    // 3. Simulation
    // =====================================================

    /// <summary>
    /// 균등 무작위 롤아웃
    /// 승패가 날 때까지(또는 안전 상한까지) 양쪽이 무작위로 카드를 사용
    /// </summary>
    private float Simulate(SimGameState state)
    {
        // 원본 상태 복사
        SimGameState sim =
            state.Clone();

        for (int step = 0; step < ROLLOUT_MAX_STEPS; step++)
        {
            // 종료 체크
            if (sim.self.CurrentHP <= 0 ||
                sim.opponent.CurrentHP <= 0)
            {
                break;
            }

            // 현재 턴 Entity
            SimEntity current =
                sim.selfTurn
                ? sim.self
                : sim.opponent;

            SimEntity target =
                sim.selfTurn
                ? sim.opponent
                : sim.self;

            // 사용 가능한 카드 중 균등 무작위 선택
            CardInstance selectedCard =
                SelectRandomCard(current);

            // 플레이 가능한 카드 없음 → 턴 종료
            if (selectedCard == null)
            {
                EndTurn(sim);
                continue;
            }

            // 카드 사용
            SimBattleHelper.TryUseCard(
                current,
                target,
                selectedCard);

            // 추가 행동 불가능하면 턴 종료
            if (!HasPlayableCard(current))
            {
                EndTurn(sim);
            }
        }

        // 롤아웃이 승패까지 도달했는지, 안전 상한에 걸렸는지 집계
        if (sim.self.CurrentHP <= 0 || sim.opponent.CurrentHP <= 0)
        {
            rolloutTerminalCount++;
        }
        else
        {
            rolloutCapCount++;
        }

        return Evaluate(sim);
    }

    private CardInstance SelectRandomCard(SimEntity entity)
    {
        CollectActionCards(entity, candidateBuffer);

        if (candidateBuffer.Count == 0)
        {
            return null;
        }

        return candidateBuffer[Random.Range(0, candidateBuffer.Count)];
    }

    // =====================================================
    // 4. Backpropagation
    // =====================================================

    private void Backpropagate(
        VanillaNode node,
        float reward)
    {
        VanillaNode current = node;

        while (current != null)
        {
            current.visitCount++;
            current.totalReward += reward;

            current = current.parent;
        }
    }

    // =====================================================
    // 최종 선택
    // =====================================================

    private VanillaNode GetMostVisitedChild(VanillaNode node)
    {
        VanillaNode best = null;

        foreach (VanillaNode child in node.children)
        {
            if (best == null ||
                child.visitCount > best.visitCount)
            {
                best = child;
            }
        }

        return best;
    }

    // =====================================================
    // 로그 기록
    // =====================================================

    /// <summary>
    /// 한 번의 카드 결정에 대한 시간 및 탐색 통계를 콘솔과 CSV에 같은 값으로 기록한다.
    /// MCTS 평균 탐색 시간(SimulationManager.RecordSearchTime)에는 섞지 않고 MCTSVanillaLogger에서 따로 집계한다.
    /// </summary>
    private void LogSearchResult(
        Entity entity,
        SimGameState rootState,
        CardInstance selectedCard,
        int playableCardCount,
        int targetIterations,
        int actualIterations,
        double elapsedMs,
        int nodeCount,
        long memoryDeltaBytes,
        bool isMemorySampleTurn)
    {
        string side =
            entity == GameData.instance.player
            ? "Player"
            : "Enemy";

        string mode =
            SimulationManager.instance != null
            ? SimulationManager.instance.MCTSModeLabel
            : "Vanilla";

        Debug.Log(
            $"[MCTSVanilla] Target: {targetIterations} | " +
            $"Actual: {actualIterations} | " +
            $"Elapsed: {elapsedMs:F3}ms | " +
            $"Nodes: {nodeCount} | " +
            $"MemDelta: {memoryDeltaBytes / 1024}KB | " +
            $"Rollout Terminal: {rolloutTerminalCount}/{rolloutTerminalCount + rolloutCapCount}" +
            (isMemorySampleTurn ? " | [MemSample]" : "")
        );

        MCTSVanillaLogger.LogSearch(
            mode,
            side,
            playableCardCount,
            rootState.self.CurrentHP,
            rootState.opponent.CurrentHP,
            targetIterations,
            actualIterations,
            elapsedMs,
            nodeCount,
            memoryDeltaBytes,
            isMemorySampleTurn,
            rolloutTerminalCount,
            rolloutCapCount,
            rootState.turnCount,
            rootState.self.Block,
            rootState.opponent.Block,
            selectedCard != null
            ? MCTSLogger.GetCardLabel(selectedCard.data, selectedCard.upgraded)
            : "None"
        );
    }

    // =====================================================
    // 보상 계산
    // =====================================================

    /// <summary>
    /// 루트 Entity 기준 승패 보상
    /// 승리 1, 패배 0, 안전 상한에 도달해 승패가 나지 않았으면 HP 비교(동률 0.5)
    /// </summary>
    private float Evaluate(SimGameState state)
    {
        // 승리
        if (state.opponent.CurrentHP <= 0) return 1f;

        // 패배
        if (state.self.CurrentHP <= 0) return 0f;

        // 안전 상한 도달 → HP 비교
        if (state.self.CurrentHP > state.opponent.CurrentHP) return 1f;

        if (state.self.CurrentHP < state.opponent.CurrentHP) return 0f;

        return 0.5f;
    }

    // =====================================================
    // 행동 후보 수집
    // =====================================================

    /// <summary>
    /// 사용 가능한 카드를 수집
    /// CardKey(카드 종류, 비용, 강화, 소멸, 휘발)가 같은 카드는 대표 1장만 후보로 둔다.
    /// </summary>
    private void CollectActionCards(
        SimEntity entity,
        List<CardInstance> result)
    {
        result.Clear();

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost > entity.CurrentEnergy)
            {
                continue;
            }

            if (ContainsSameCard(result, card))
            {
                continue;
            }

            result.Add(card);
        }
    }

    private bool ContainsSameCard(
        List<CardInstance> cards,
        CardInstance card)
    {
        foreach (CardInstance other in cards)
        {
            if (other.data == card.data &&
                other.currentCost == card.currentCost &&
                other.upgraded == card.upgraded &&
                other.exhaust == card.exhaust &&
                other.ethereal == card.ethereal)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================================
    // 플레이 가능한 카드 존재 여부
    // =====================================================

    private bool HasPlayableCard(SimEntity entity)
    {
        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================================
    // instanceID 기반 카드 탐색
    // =====================================================

    private CardInstance FindCardByID(
        SimEntity entity,
        int instanceID)
    {
        foreach (CardInstance card in entity.hand)
        {
            if (card.instanceID == instanceID)
            {
                return card;
            }
        }

        return null;
    }

    // =====================================================
    // 턴 종료 처리
    // =====================================================

    private void EndTurn(SimGameState sim)
    {
        // =================================================
        // 현재 턴 플레이어
        // =================================================

        SimEntity current =
            sim.selfTurn
            ? sim.self
            : sim.opponent;

        // =================================================
        // 턴 종료 처리
        // =================================================

        // 에너지 제거
        current.CurrentEnergy = 0;

        // 손패 버리기
        current.DiscardHand();

        // 상태 감소
        current.TickStatuses();

        // =================================================
        // 턴 전환
        // =================================================

        sim.selfTurn = !sim.selfTurn;

        sim.turnCount++;

        // =================================================
        // 다음 플레이어
        // =================================================

        SimEntity next =
            sim.selfTurn
            ? sim.self
            : sim.opponent;

        // =================================================
        // 턴 시작 처리
        // =================================================

        // Block 제거
        next.Block = 0;

        // 에너지 회복
        next.CurrentEnergy =
            next.MaxEnergy;

        // 카드 드로우
        next.DrawCards(5);
    }
}
