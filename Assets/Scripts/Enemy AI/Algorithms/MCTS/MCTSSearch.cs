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

    private const bool USE_FIXED_ITERATIONS_FOR_TEST = false; // true로 설정하면 테스트용으로 고정된 반복 횟수를 사용, false로 설정하면 동적 반복 횟수 계산 사용
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
        => (USE_FIXED_ITERATIONS_FOR_TEST ? "Fixed" : "Dynamic") +
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

        // ===== 즉시 처치 가능 여부 우선 확인 =====
        bool lethalCheckOn =
            SimulationManager.instance == null || SimulationManager.instance.EnableLethalCheck;

        if (lethalCheckOn)
        {
            List<MCTSAction> lethalSequence =
                MCTSLethalChecker.FindLethalSequence(rootState);

            if (lethalSequence != null && lethalSequence.Count > 0)
            {
                return lethalSequence[0];
            }
        }

        MCTSNode root =
            new MCTSNode(rootState);

        // 새 탐색 트리 시작 시 캐시 초기화
        MCTSTranspositionTable.Clear();

        // =================================================
        // 반복 횟수 결정
        // =================================================

        int iterations =
            USE_FIXED_ITERATIONS_FOR_TEST
            ? FIXED_TEST_ITERATIONS
            : GetDynamicIterations(rootState, playableCardCount);

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

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        int executedIterations = 0;

        for (int i = 0; i < iterations; i++)
        {
            executedIterations = i + 1;

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

            float reward =
                MCTSSimulation.Simulate(simulationNode.state); // 해싱 검증 전까지 캐싱 경로 비활성화

            // 4. Backpropagation
            MCTSBackpropagation.Backpropagate(
                simulationNode,
                reward
            );

            // 5. 조기 종료 판단
            if (!DISABLE_EARLY_STOP_FOR_TEST &&
                i >= MIN_ITERATIONS &&
                i % CONVERGENCE_CHECK_INTERVAL == 0 &&
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

        // 팀원 SimulationManager의 평균 탐색 시간 통계에도 반영
        SimulationManager.RecordSearchTime(stopwatch.ElapsedMilliseconds);

        // =================================================
        // 벤치마크 로그 (항상 출력)
        // =================================================

        Debug.Log(
            $"[MCTS] Target: {iterations} | " +
            $"Actual: {executedIterations} | " +
            $"Elapsed: {stopwatch.ElapsedMilliseconds}ms | " +
            $"Nodes: {nodeCount} | " +
            $"MemDelta: {memoryDeltaBytes / 1024}KB" +
            (isMemorySampleTurn ? " | [MemSample]" : "")
        );

        // CSV 파일로도 기록
        MCTSLogger.LogSearch(
            CurrentMode,
            playableCardCount,
            rootState.self.CurrentHP,
            rootState.opponent.CurrentHP,
            iterations,
            executedIterations,
            stopwatch.ElapsedMilliseconds,
            nodeCount,
            memoryDeltaBytes,
            isMemorySampleTurn
        );

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
