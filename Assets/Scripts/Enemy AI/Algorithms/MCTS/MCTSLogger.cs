using System;
using System.IO;
using UnityEngine;

/// <summary>
/// MCTS 탐색 결과를 CSV 파일로 기록
/// 시뮬레이션 한 회차가 끝날 때마다 별도 파일로 분리 저장
/// </summary>
public static class MCTSLogger
{
    // =====================================================
    // 진행 중인 시뮬레이션의 임시 파일 경로
    // =====================================================

    private static string currentMode = "Unknown";

    private static string searchLogPath =
        Path.Combine(Application.persistentDataPath, "_current_search_log.csv");

    private static string battleResultPath =
        Path.Combine(Application.persistentDataPath, "_current_battle_results.csv");

    // =====================================================
    // 시뮬레이션 회차 관리
    // =====================================================

    /// <summary>
    /// 새 시뮬레이션 회차 시작. 이전 임시 파일이 남아있으면 삭제하고 새로 시작
    /// </summary>
    public static void StartNewRun(string mode)
    {
        currentMode = mode;

        if (File.Exists(searchLogPath))
        {
            File.Delete(searchLogPath);
        }

        if (File.Exists(battleResultPath))
        {
            File.Delete(battleResultPath);
        }
    }

    /// <summary>
    /// 시뮬레이션 회차 종료. 임시 파일을 종료 시간이 담긴 최종 파일명으로 변경
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
                    $"{currentMode}_Simulation_{timestamp}_search.csv");

            File.Move(searchLogPath, finalPath);
        }

        if (File.Exists(battleResultPath))
        {
            string finalPath =
                Path.Combine(
                    Application.persistentDataPath,
                    $"{currentMode}_Simulation_{timestamp}_battles.csv");

            File.Move(battleResultPath, finalPath);
        }
    }

    // =====================================================
    // 턴 단위 로그 기록
    // =====================================================

    /// <summary>
    /// MCTS 탐색 결과 한 턴 분량을 CSV 파일에 한 줄 추가
    /// </summary>
    public static void LogSearch(
        string mode,
        int playableCardCount,
        int selfHP,
        int opponentHP,
        int targetIterations,
        int actualIterations,
        long elapsedMs)
    {
        bool fileExists =
            File.Exists(searchLogPath);

        using (StreamWriter writer =
            new StreamWriter(searchLogPath, append: true))
        {
            // 파일이 처음 생성되는 경우 헤더 작성
            if (!fileExists)
            {
                writer.WriteLine(
                    "Timestamp,BattleNumber,Mode,PlayableCardCount," +
                    "SelfHP,OpponentHP,HPDifference," +
                    "TargetIterations,ActualIterations,ElapsedMs"
                );
            }

            int battleNumber =
                GetCurrentBattleNumber();

            int hpDifference =
                Mathf.Abs(selfHP - opponentHP);

            writer.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
                $"{battleNumber}," +
                $"{mode}," +
                $"{playableCardCount}," +
                $"{selfHP}," +
                $"{opponentHP}," +
                $"{hpDifference}," +
                $"{targetIterations}," +
                $"{actualIterations}," +
                $"{elapsedMs}"
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

    // =====================================================
    // 승패 결과 기록
    // =====================================================

    /// <summary>
    /// 한 판의 승패 결과를 CSV 파일에 한 줄 추가
    /// </summary>
    public static void LogBattleResult(
        int battleNumber,
        string mode,
        bool playerWin)
    {
        bool fileExists =
            File.Exists(battleResultPath);

        using (StreamWriter writer =
            new StreamWriter(battleResultPath, append: true))
        {
            // 파일이 처음 생성되는 경우 헤더 작성
            if (!fileExists)
            {
                writer.WriteLine(
                    "Timestamp,BattleNumber,Mode,Winner"
                );
            }

            string winner =
                playerWin ? "Player" : "Enemy";

            writer.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
                $"{battleNumber}," +
                $"{mode}," +
                $"{winner}"
            );
        }
    }
}
