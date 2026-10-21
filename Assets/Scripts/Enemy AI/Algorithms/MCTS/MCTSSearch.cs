using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

/// <summary>
/// MCTS 전체 탐색 수행
/// </summary>
public static class MCTSSearch
{
    // =====================================================
    // 설정값
    // =====================================================

    private const int MIN_ITERATIONS = 200; // 최소 반복 횟수, 이 횟수 이상은 반드시 반복 수행
    private const int MAX_ITERATIONS = 1000; // 최대 반복 횟수, 이 횟수 이상은 반복 수행하지 않음

    private const int ITERATIONS_PER_ACTION = 150; // 선택지 1개당 추가 반복 횟수, 선택지가 많을수록 더 많은 반복 수행

    private const int CLOSE_FIGHT_MAX_BONUS = 300; // HP 차이가 0일 때 추가 반복 횟수, 접전일수록 더 많은 반복 수행
    private const float CLOSE_FIGHT_ZERO_THRESHOLD_RATIO = 0.8f; // 최대 HP 대비 이 비율만큼 차이나면 접전 보너스가 0이 됨 (기존 2/3 비율이 너무 엄격해서 0.8로 완화)

    private const int CONVERGENCE_CHECK_INTERVAL = 100; // 조기 종료 판단을 수행할 반복 횟수 간격, 이 횟수마다 수렴 여부를 판단
    private const float CONVERGENCE_RATIO = 2f; // 최다 방문 child가 2위 child보다 몇 배 이상 방문되었을 때 수렴한 것으로 판단

    // =====================================================
    // 테스트 / 디버그용 설정
    // =====================================================

    private const bool LOG_CHILD_STATS = false; // true로 설정하면 루트 노드의 자식 노드들의 통계 로그를 출력

    private const bool USE_FIXED_ITERATIONS_FOR_TEST = false; // SimulationManager가 없을 때만 사용하는 기본 테스트값
    private const int FIXED_TEST_ITERATIONS = 1000;

    private const bool DISABLE_EARLY_STOP_FOR_TEST = false; // true로 설정하면 조기 종료를 사용하지 않음, 순수 반복 횟수만으로 비교할 때 사용

    private const bool FORCE_GC_FOR_MEMORY_TEST = false; // true로 설정하면 매 턴 강제 GC 수행 (메모리 전용 테스트에서만 사용, 평소엔 false)

    private const int MEMORY_SAMPLE_INTERVAL = 20; // 이 턴 수마다 한 번씩만 강제 GC로 정확한 메모리 측정 (해당 턴의 ElapsedMs는 왜곡되므로 시간 분석에서 제외할 것)

    private static int searchCallCount = 0; // Search() 호출 횟수 누적, 메모리 샘플링 주기 판단용

    // =====================================================
    // 외부 참조용
    // =====================================================

    /// <summary>
    /// 현재 반복 횟수 계산 방식 (승패 로그 기록 시 참조)
    /// </summary>
    public static string CurrentMode
        => SimulationManager.instance != null
            ? SimulationManager.instance.MCTSModeLabel
            : (USE_FIXED_ITERATIONS_FOR_TEST ? "Fixed" : "Dynamic") +
           (DISABLE_EARLY_STOP_FOR_TEST ? "_NoEarlyStop" : "") +
           (FORCE_GC_FOR_MEMORY_TEST ? "_MemTest" : "");

    // =====================================================
    // Search
    // =====================================================

    /// <summary>
    /// 현재 상태에서 가장 좋은 action 선택
    /// </summary>
    public static MCTSAction Search(Entity entity)
    {
        MCTSNode chosenNode;

        return Search(entity, null, out chosenNode);
    }

    /// <summary>
    /// 현재 상태에서 가장 좋은 action 선택 (Tree Reuse 지원)
    /// reusableNode: 직전 탐색에서 고른 child 노드, 같은 턴의 다음 탐색에서 루트로 재사용을 시도
    /// chosenNode: 이번 탐색에서 고른 child 노드, Tree Reuse가 꺼져 있으면 null
    /// </summary>
    public static MCTSAction Search(
        Entity entity,
        MCTSNode reusableNode,
        out MCTSNode chosenNode)
    {
        chosenNode = null;

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
            return default;
        }

        // =================================================
        // 루트 상태 생성
        // =================================================

        SimGameState rootState =
            SimGameState.Create(entity);

        // =================================================
        // 탐색 예산 결정
        // =================================================

        bool useFixedBudget =
            SimulationManager.instance != null
            ? SimulationManager.instance.UseFixedSimulationBudget
            : USE_FIXED_ITERATIONS_FOR_TEST;

        bool useTimeBudget =
            SimulationManager.instance != null &&
            SimulationManager.instance.UseTimeBudget;

        int iterations =
            useFixedBudget
            ? (SimulationManager.instance != null
                ? SimulationManager.instance.FixedSimulationBudget
                : FIXED_TEST_ITERATIONS)
            : (useTimeBudget ? -1 : GetDynamicIterations(rootState, playableCardCount));

        double timeBudgetMs =
            useTimeBudget
            ? SimulationManager.instance.TimeBudgetMilliseconds
            : 0d;

        string budgetMode = useTimeBudget
            ? "TimeLimit"
            : (useFixedBudget ? "FixedSimulationCount" : "Dynamic");

        // =================================================
        // MCTS 반복
        // =================================================

        searchCallCount++;

        bool isMemorySampleTurn =
            FORCE_GC_FOR_MEMORY_TEST ||
            (searchCallCount % MEMORY_SAMPLE_INTERVAL == 0);

        long memoryBefore =
            isMemorySampleTurn
            ? System.GC.GetTotalMemory(true)
            : System.GC.GetTotalMemory(false);

        int nodeCount = 0;
        int cacheHitsBefore = MCTSTranspositionTable.hitCount;
        int cacheLookupsBefore = MCTSTranspositionTable.totalLookups;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        // ===== 즉시 처치 가능 여부 우선 확인 =====
        // Lethal Check의 계산 시간과 사용 여부도 실험 로그에 포함한다.
        bool lethalSequenceUsed = false;
        bool lethalCheckOn =
            SimulationManager.instance == null || SimulationManager.instance.EnableLethalCheck;

        if (lethalCheckOn)
        {
            List<MCTSAction> lethalSequence =
                MCTSLethalChecker.FindLethalSequence(rootState);

            if (lethalSequence != null && lethalSequence.Count > 0)
            {
                lethalSequenceUsed = true;
                stopwatch.Stop();

                long lethalMemoryAfter =
                    isMemorySampleTurn
                    ? System.GC.GetTotalMemory(true)
                    : System.GC.GetTotalMemory(false);

                LogSearchResult(
                    entity,
                    rootState,
                    lethalSequence[0],
                    playableCardCount,
                    budgetMode,
                    iterations,
                    timeBudgetMs,
                    0,
                    stopwatch.Elapsed.TotalMilliseconds,
                    0,
                    lethalMemoryAfter - memoryBefore,
                    isMemorySampleTurn,
                    true,
                    cacheHitsBefore,
                    cacheLookupsBefore,
                    false,
                    0);

                return lethalSequence[0];
            }
        }

        // =================================================
        // 루트 노드 준비 (Tree Reuse)
        // =================================================

        bool treeReuseOn =
            SimulationManager.instance == null || SimulationManager.instance.EnableTreeReuse;

        int reusedNodeCount = 0;

        MCTSNode root =
            treeReuseOn
            ? TryReuseTree(reusableNode, rootState, out reusedNodeCount)
            : null;

        bool treeReused = root != null;

        // 재사용할 트리가 없으면 새 트리로 시작
        if (root == null)
        {
            root = new MCTSNode(rootState);
        }

        int executedIterations = 0;

        while (useTimeBudget || executedIterations < iterations)
        {
            // 시간 예산 모드에서는 최소 한 번 탐색한 뒤 제한 시간을 확인한다.
            if (useTimeBudget &&
                executedIterations > 0 &&
                stopwatch.Elapsed.TotalMilliseconds >= timeBudgetMs)
            {
                break;
            }

            executedIterations++;

            // 1. Selection
            MCTSNode selected =
                MCTSSelection.Select(root);

            if (selected == null || selected.state == null)
                continue;

            // 2. Expansion
            MCTSNode expanded =
                MCTSExpansion.Expand(selected);

            if (expanded != null)
                nodeCount++;

            // 3. Simulation
            MCTSNode simulationNode =
                expanded ?? selected;

            if (simulationNode == null || simulationNode.state == null)
                continue;

            // 노드 전체를 전달해야 상태 해시를 통한 캐시 조회/저장이 수행된다.
            float reward =
                MCTSSimulation.Simulate(simulationNode);

            // 4. Backpropagation
            MCTSBackpropagation.Backpropagate(
                simulationNode,
                reward
            );

            // 5. 조기 종료 판단
            bool searchEarlyStopOn =
                SimulationManager.instance != null
                ? SimulationManager.instance.EnableSearchEarlyStop
                : !DISABLE_EARLY_STOP_FOR_TEST;

            if (searchEarlyStopOn &&
                executedIterations >= MIN_ITERATIONS &&
                executedIterations % CONVERGENCE_CHECK_INTERVAL == 0 &&
                IsConverged(root))
            {
                break;
            }
        }

        stopwatch.Stop();

        long memoryAfter =
            isMemorySampleTurn
            ? System.GC.GetTotalMemory(true)
            : System.GC.GetTotalMemory(false);

        long memoryDeltaBytes =
            memoryAfter - memoryBefore;

        // 최다 방문 child 선택 (선택 카드를 로그에 남기기 위해 기록 전에 계산, 시간 측정 구간 밖)
        MCTSNode bestChild =
            root.GetMostVisitedChild();

        LogSearchResult(
            entity,
            rootState,
            bestChild != null ? bestChild.actionFromParent : default,
            playableCardCount,
            budgetMode,
            iterations,
            timeBudgetMs,
            executedIterations,
            stopwatch.Elapsed.TotalMilliseconds,
            nodeCount,
            memoryDeltaBytes,
            isMemorySampleTurn,
            lethalSequenceUsed,
            cacheHitsBefore,
            cacheLookupsBefore,
            treeReused,
            reusedNodeCount);
        // =================================================
        // 최종 선택
        // =================================================

        if (LOG_CHILD_STATS)
        {
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
        }

        if (bestChild == null)
        {
            return default;
        }

        // Tree Reuse가 켜져 있으면 고른 child를 다음 탐색의 루트 후보로 넘김
        if (treeReuseOn)
        {
            chosenNode = bestChild;
        }

        // =================================================
        // Action 반환
        // =================================================

        return bestChild.actionFromParent;
    }

    /// <summary>
    /// 한 번의 카드 결정에 대한 시간 및 탐색 통계를 콘솔과 CSV에 같은 값으로 기록한다.
    /// Lethal Check로 즉시 카드를 고른 경우도 이 경로를 거쳐 비교에서 누락되지 않는다.
    /// </summary>
    private static void LogSearchResult(
        Entity entity,
        SimGameState rootState,
        MCTSAction selectedAction,
        int playableCardCount,
        string budgetMode,
        int targetIterations,
        double timeBudgetMs,
        int actualIterations,
        double elapsedMs,
        int nodeCount,
        long memoryDeltaBytes,
        bool isMemorySampleTurn,
        bool lethalSequenceUsed,
        int cacheHitsBefore,
        int cacheLookupsBefore,
        bool treeReused,
        int reusedNodeCount)
    {
        int cacheHitsThisSearch =
            MCTSTranspositionTable.hitCount - cacheHitsBefore;
        int cacheLookupsThisSearch =
            MCTSTranspositionTable.totalLookups - cacheLookupsBefore;

        SimulationManager.RecordSearchTime(elapsedMs);

        string targetDescription = budgetMode == "TimeLimit"
            ? $"{timeBudgetMs:F3}ms"
            : targetIterations.ToString();

        Debug.Log(
            $"[MCTS] Target: {targetDescription} | " +
            $"Actual: {actualIterations} | " +
            $"Elapsed: {elapsedMs:F3}ms | " +
            $"Nodes: {nodeCount} | " +
            $"MemDelta: {memoryDeltaBytes / 1024}KB | " +
            $"Cache: {MCTSTranspositionTable.hitCount}/" +
            $"{MCTSTranspositionTable.totalLookups} " +
            $"({MCTSTranspositionTable.HitRate * 100f:F1}%) | " +
            $"Entries: {MCTSTranspositionTable.EntryCount}" +
            (treeReused ? $" | [Reuse:{reusedNodeCount}]" : "") +
            (lethalSequenceUsed ? " | [Lethal]" : "") +
            (isMemorySampleTurn ? " | [MemSample]" : "")
        );

        MCTSLogger.LogSearch(
            CurrentMode,
            playableCardCount,
            rootState.self.CurrentHP,
            rootState.opponent.CurrentHP,
            budgetMode,
            targetIterations,
            timeBudgetMs,
            actualIterations,
            elapsedMs,
            nodeCount,
            memoryDeltaBytes,
            isMemorySampleTurn,
            lethalSequenceUsed,
            cacheHitsThisSearch,
            cacheLookupsThisSearch,
            MCTSTranspositionTable.EntryCount,
            treeReused,
            reusedNodeCount,
            entity == GameData.instance.player ? "Player" : "Enemy",
            rootState.turnCount,
            rootState.self.Block,
            rootState.opponent.Block,
            MCTSLogger.GetCardLabel(selectedAction.cardKey.data, selectedAction.cardKey.upgraded)
        );
    }

    // =====================================================
    // Tree Reuse + Fast Expansion
    // =====================================================

    /// <summary>
    /// 직전 탐색에서 고른 child 노드를 이번 탐색의 루트로 재사용
    /// 실제 게임 상태와 같을 때만 재사용하고, 다르면 null을 반환해 새 트리로 시작하게 함
    /// </summary>
    private static MCTSNode TryReuseTree(
        MCTSNode candidate,
        SimGameState rootState,
        out int reusedNodeCount)
    {
        reusedNodeCount = 0;

        if (candidate == null || candidate.state == null)
            return null;

        // 카드 사용 후 턴이 끝난 노드는 다음 턴 상태와 맞지 않으므로 재사용하지 않음
        if (!candidate.state.selfTurn)
            return null;

        // 실제 탐색의 루트는 turnHistory가 매번 초기화되므로,
        // 양쪽 모두 후보 노드의 turnHistory로 해시를 계산해 게임 상태만 비교
        ulong candidateHash =
            candidate.state.GetStateHash(candidate.turnHistory);

        ulong actualHash =
            rootState.GetStateHash(candidate.turnHistory);

        if (candidateHash != actualHash)
            return null;

        // 고를 수 있는 행동이 없는 노드는 재사용하지 않음
        // 재사용 노드는 이번 턴 사용 이력을 이어받아 확장 단계 가지치기가 루트에도 적용되므로,
        // 남은 카드가 모두 제외되면 카드를 남긴 채 턴이 끝남. 새 트리로 시작해 Tree Reuse off와 같은 결정 규칙을 따르게 함
        if (candidate.IsFullyExpanded() &&
            candidate.children.Count == 0)
            return null;

        // 이전 루트와 연결을 끊음 (역전파가 이전 루트까지 올라가지 않고, 이전 트리가 메모리에서 해제되도록)
        candidate.parent = null;

        reusedNodeCount =
            ApplyFastExpansion(candidate);

        return candidate;
    }

    /// <summary>
    /// Fast Expansion: 재사용하는 서브트리에서 방문한 노드마다
    /// 평균 보상은 유지하고 방문 수는 1로 초기화 (이전 탐색 통계가 과도한 영향을 주지 않도록)
    /// 초기화한 노드 수를 반환
    /// </summary>
    private static int ApplyFastExpansion(MCTSNode node)
    {
        int count = 0;

        if (node.visitCount > 0)
        {
            node.totalReward = node.AverageReward;
            node.visitCount = 1;
            count++;
        }

        foreach (MCTSNode child in node.children)
        {
            count += ApplyFastExpansion(child);
        }

        return count;
    }

    // =====================================================
    // 동적 반복 횟수 계산
    // =====================================================

    /// <summary>
    /// 현재 상황(선택지 수, HP 차이)에 따라 반복 횟수를 동적으로 계산
    /// </summary>
    private static int GetDynamicIterations(
        SimGameState rootState,
        int playableCardCount)
    {
        // 선택지가 많을수록 더 많은 반복이 필요
        int actionBonus =
            (playableCardCount - 1) * ITERATIONS_PER_ACTION;

        // HP 차이가 적을수록(접전일수록) 더 많은 반복이 필요
        int hpDifference =
            Mathf.Abs(rootState.self.CurrentHP - rootState.opponent.CurrentHP);

        // 최대 HP가 얼마든 항상 같은 비율 기준으로 접전 여부를 판단하기 위해 정규화
        int maxHP =
            Mathf.Max(rootState.self.MaxHP, rootState.opponent.MaxHP);

        float hpDifferenceRatio =
            maxHP > 0 ? (float)hpDifference / maxHP : 0f;

        int closeFightBonus =
            Mathf.RoundToInt(
                CLOSE_FIGHT_MAX_BONUS *
                Mathf.Max(0f, 1f - hpDifferenceRatio / CLOSE_FIGHT_ZERO_THRESHOLD_RATIO)
            );

        int iterations =
            MIN_ITERATIONS + actionBonus + closeFightBonus;

        return Mathf.Clamp(iterations, MIN_ITERATIONS, MAX_ITERATIONS);
    }

    // =====================================================
    // 조기 종료 판단
    // =====================================================

    /// <summary>
    /// 최다 방문 child가 2위 child를 압도적으로 앞서면 수렴한 것으로 판단
    /// </summary>
    private static bool IsConverged(MCTSNode root)
    {
        if (root.children == null || root.children.Count < 2)
            return false;

        int bestVisit = -1;
        int secondVisit = -1;

        foreach (MCTSNode child in root.children)
        {
            if (child.visitCount > bestVisit)
            {
                secondVisit = bestVisit;
                bestVisit = child.visitCount;
            }
            else if (child.visitCount > secondVisit)
            {
                secondVisit = child.visitCount;
            }
        }

        if (secondVisit <= 0)
            return false;

        return bestVisit >= secondVisit * CONVERGENCE_RATIO;
    }
}
