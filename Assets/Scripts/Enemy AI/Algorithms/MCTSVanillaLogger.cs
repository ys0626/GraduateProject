using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// 바닐라 MCTS 탐색 결과를 CSV 파일로 기록
/// MCTSLogger와 같은 구조로, 시뮬레이션 한 회차가 끝날 때마다 별도 파일로 분리 저장
/// 승패 결과는 MCTSLogger의 battles 파일에 대결 구도 라벨과 함께 기록되므로 여기서는 탐색 결과만 기록
/// </summary>
public static class MCTSVanillaLogger
{
    // =====================================================
    // 진행 중인 시뮬레이션의 임시 파일 경로
    // =====================================================

    private static string currentMode = "Unknown";

    private static string searchLogPath =
        Path.Combine(Application.persistentDataPath, "_current_vanilla_search_log.csv");

    // =====================================================
    // 평균 탐색 시간 집계
    // =====================================================

    // SimulationManager의 MCTS 평균 탐색 시간과 섞이지 않도록 따로 집계
    private static double totalSearchTimeMs;
    private static int searchCount;

    public static double AverageSearchTimeMs =>
        searchCount > 0 ? totalSearchTimeMs / searchCount : 0d;

    public static int SearchCount => searchCount;

    // =====================================================
    // 시뮬레이션 회차 관리
    // =====================================================

    /// <summary>
    /// 새 시뮬레이션 회차 시작. 이전 임시 파일이 남아있으면 삭제하고 새로 시작
    /// </summary>
    public static void StartNewRun(string mode)
    {
        currentMode = mode;

        totalSearchTimeMs = 0d;
        searchCount = 0;

        if (File.Exists(searchLogPath))
        {
            File.Delete(searchLogPath);
        }
    }

    /// <summary>
    /// 시뮬레이션 회차 종료. 임시 파일을 종료 시간이 담긴 최종 파일명으로 변경
    /// 바닐라가 한 번도 탐색하지 않았으면 파일이 없으므로 아무것도 하지 않음
    /// </summary>
    public static void FinishRun()
    {
        string timestamp =
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        if (File.Exists(searchLogPath))
        {
            string finalPath =
                Path.Combine(
                    Application.persistentDataPath,
                    $"{currentMode}_Simulation_{timestamp}_vanilla_search.csv");

            File.Move(searchLogPath, finalPath);
        }
    }

    // =====================================================
    // 탐색 단위 로그 기록
    // =====================================================

    /// <summary>
    /// 바닐라 MCTS 탐색 결과 한 번 분량을 CSV 파일에 한 줄 추가
    /// MCTSLogger와 비교할 수 있도록 공통 컬럼은 같은 이름과 순서를 사용
    /// </summary>
    public static void LogSearch(
        string mode,
        string side,
        int playableCardCount,
        int selfHP,
        int opponentHP,
        int targetIterations,
        int actualIterations,
        double elapsedMs,
        int nodeCount,
        long memoryDeltaBytes,
        bool isMemorySample,
        int rolloutTerminalCount,
        int rolloutCapCount,
        int turnNumber,
        int selfBlock,
        int opponentBlock,
        string selectedCard)
    {
        totalSearchTimeMs += elapsedMs;
        searchCount++;

        bool fileExists =
            File.Exists(searchLogPath);

        using (StreamWriter writer =
            new StreamWriter(searchLogPath, append: true))
        {
            // 파일이 처음 생성되는 경우 헤더 작성
            if (!fileExists)
            {
                writer.WriteLine(
                    "Timestamp,BattleNumber,Mode,Side,PlayableCardCount," +
                    "SelfHP,OpponentHP,HPDifference," +
                    "BudgetMode,TargetIterations,ActualIterations,ElapsedMs," +
                    "NodeCount,MemoryDeltaBytes,IsMemorySample," +
                    "RolloutTerminalCount,RolloutCapCount,RolloutTerminalRate," +
                    "TurnNumber,SelfBlock,OpponentBlock,SelectedCard"
                );
            }

            int battleNumber =
                GetCurrentBattleNumber();

            int hpDifference =
                Mathf.Abs(selfHP - opponentHP);

            int rolloutTotal =
                rolloutTerminalCount + rolloutCapCount;

            float rolloutTerminalRate =
                rolloutTotal > 0 ? (float)rolloutTerminalCount / rolloutTotal : 0f;

            writer.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
                $"{battleNumber}," +
                $"{mode}," +
                $"{side}," +
                $"{playableCardCount}," +
                $"{selfHP}," +
                $"{opponentHP}," +
                $"{hpDifference}," +
                $"FixedSimulationCount," +
                $"{targetIterations}," +
                $"{actualIterations}," +
                $"{elapsedMs.ToString("F4", CultureInfo.InvariantCulture)}," +
                $"{nodeCount}," +
                $"{memoryDeltaBytes}," +
                $"{isMemorySample}," +
                $"{rolloutTerminalCount}," +
                $"{rolloutCapCount}," +
                $"{rolloutTerminalRate.ToString("F4", CultureInfo.InvariantCulture)}," +
                $"{turnNumber}," +
                $"{selfBlock}," +
                $"{opponentBlock}," +
                $"{selectedCard}"
            );
        }
    }

    // =====================================================
    // 현재 판(battle) 번호 조회
    // =====================================================

    /// <summary>
    /// SimulationManager가 있으면 진행 중인 판 번호를, 없으면 -1을 반환
    /// </summary>
    private static int GetCurrentBattleNumber()
    {
        if (SimulationManager.instance == null)
            return -1;

        return SimulationManager.instance.CurrentBattle + 1;
    }
}
