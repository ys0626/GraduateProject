using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 동적 탐색을 끈 MCTS가 사용할 탐색 예산 방식
/// </summary>
public enum MCTSSearchBudgetMode
{
    FixedSimulationCount,
    TimeLimit
}

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance;

    [Header("Player Setting")]
    [SerializeField] public CardData[] playerStarterDeck;
    [SerializeField] public int playerMaxHP;
    [SerializeField] public int playerMaxEnergy;

    [Header("Enemy Setting")]
    [SerializeField] public CardData[] enemyStarterDeck;
    [SerializeField] public int enemyMaxHP;
    [SerializeField] public int enemyMaxEnergy;

    [Header("Controller Setting")]
    [SerializeField] private ControllerType playerControllerType;
    [SerializeField] private ControllerType enemyControllerType;

    [Header("Simulation")]
    [SerializeField] private bool autoSimulation;

    [SerializeField] private int totalBattleCount;

    [Header("Speed")]
    [SerializeField] private float timeScale;

    // MCTS 컨트롤러에만 적용되며, MCTSVanilla는 이 값들을 참조하지 않음
    [Header("MCTS Techniques")]
    [Tooltip("선택지 수와 HP 차이로 반복 횟수를 계산합니다. 끄면 아래 Search Budget을 사용합니다.")]
    [SerializeField] private bool enableDynamicSearch = true;
    [Tooltip("롤아웃에서 4가지 필터 규칙과 Power 카드 가중치를 사용합니다. 끄면 사용 가능한 카드 중 균등 무작위로 고릅니다.")]
    [SerializeField] private bool enableHeuristicRollout = true;
    [Tooltip("최다 방문 child가 2위 child보다 일정 배수 이상 방문되면 탐색을 중단합니다.")]
    [SerializeField] private bool enableEarlyStop = true;
    [Tooltip("전위 테이블 캐시와, 일정 횟수 이상 방문한 노드의 시뮬레이션 생략을 함께 사용합니다.")]
    [SerializeField] private bool enableCaching = true;
    [SerializeField][Range(5, 200)] private int earlyCutoffVisitThreshold = 30; // 캐싱의 시뮬레이션 생략 기준 방문 수
    [Tooltip("같은 턴의 다음 탐색에서 이전 트리를 재사용하고, 재사용 노드의 방문 수를 1로 초기화합니다(Fast Expansion).")]
    [SerializeField] private bool enableTreeReuse = true;

    [Header("MCTS Etc")]
    [Tooltip("트리 확장 단계에서 휴리스틱 규칙에 걸리는 카드를 후보에서 제외합니다.")]
    [SerializeField] private bool enableHeuristicActionPruning = true;
    [Tooltip("탐색 전에 이번 턴 안에 상대를 처치할 수 있는 카드 순서가 있는지 먼저 확인합니다.")]
    [SerializeField] private bool enableLethalCheck = true;

    [Header("MCTS Evaluation")]
    [Tooltip("승리 1, 패배 -1, 비종결 상태는 tanh 결과에 0.9를 곱하고 탐험 상수 2√2를 사용합니다. 끄면 기존 평가(승리 0.05, 패배 -0.05, tanh, 탐험 상수 1)를 사용합니다.")]
    [SerializeField] private bool enableImprovedEvaluation = false;

    [Header("Search Budget")]
    [Tooltip("MCTS는 동적 탐색을 끈 경우에만 사용합니다. MCTSVanilla는 항상 Fixed Simulation Budget을 사용합니다.")]
    [SerializeField] private MCTSSearchBudgetMode searchBudgetMode = MCTSSearchBudgetMode.FixedSimulationCount;
    [Min(1)] [SerializeField] private int fixedSimulationBudget = 100;
    [Min(0.1f)] [SerializeField] private float timeBudgetMilliseconds = 5f;

    private int currentBattle;

    private int playerWinCount;
    private int enemyWinCount;

    private static double totalSearchTimeMs;
    private static int searchCallCount;
    public static double AverageSearchTimeMs =>
        searchCallCount > 0 ? totalSearchTimeMs / searchCallCount : 0d;

    [Header("Getter")]
    public bool AutoSimulation => autoSimulation;
    public CardData[] PlayerStarterDeck
    => playerStarterDeck;

    public int PlayerMaxHP
        => playerMaxHP;

    public int PlayerMaxEnergy
        => playerMaxEnergy;

    public CardData[] EnemyStarterDeck
        => enemyStarterDeck;

    public int EnemyMaxHP
        => enemyMaxHP;

    public int EnemyMaxEnergy
        => enemyMaxEnergy;

    public ControllerType PlayerControllerType
        => playerControllerType;

    public ControllerType EnemyControllerType
        => enemyControllerType;

    // ===== MCTS 기법 =====
    public bool EnableDynamicSearch => enableDynamicSearch;
    public bool EnableHeuristicRollout => enableHeuristicRollout;
    public bool EnableSearchEarlyStop => enableEarlyStop;
    public bool EnableTranspositionCache => enableCaching; // 캐싱: 전위 테이블
    public bool EnableEarlyCutoff => enableCaching; // 캐싱: 시뮬레이션 생략
    public int EarlyCutoffVisitThreshold => earlyCutoffVisitThreshold;
    public bool EnableTreeReuse => enableTreeReuse;

    // ===== MCTS 기타 =====
    public bool EnableHeuristicActionPruning => enableHeuristicActionPruning;
    public bool EnableLethalCheck => enableLethalCheck;

    // ===== MCTS 평가 =====
    public bool EnableImprovedEvaluation => enableImprovedEvaluation;

    // ===== 탐색 예산 (동적 탐색이 꺼진 경우에만 MCTS에 적용) =====
    public bool UseFixedSimulationBudget =>
        !enableDynamicSearch && searchBudgetMode == MCTSSearchBudgetMode.FixedSimulationCount;
    public bool UseTimeBudget =>
        !enableDynamicSearch && searchBudgetMode == MCTSSearchBudgetMode.TimeLimit;
    public int FixedSimulationBudget => Mathf.Max(1, fixedSimulationBudget); // MCTSVanilla도 이 값을 사용
    public double TimeBudgetMilliseconds => Mathf.Max(0.1f, timeBudgetMilliseconds);

    /// <summary>
    /// 대결 구도와 MCTS 기법 on/off 조합을 나타내는 라벨 (CSV 파일명, Mode 컬럼에 사용)
    /// 예: MCTS_vs_MCTSVanilla_DS1_HR1_ES1_TC1_TR1_LC1_AP1_EV0_Vanilla100
    /// </summary>
    public string MCTSModeLabel
    {
        get
        {
            string label =
                $"{playerControllerType}_vs_{enemyControllerType}_" +
                $"DS{(enableDynamicSearch ? 1 : 0)}_" +
                $"HR{(enableHeuristicRollout ? 1 : 0)}_" +
                $"ES{(enableEarlyStop ? 1 : 0)}_" +
                $"TC{(enableCaching ? 1 : 0)}_" +
                $"TR{(enableTreeReuse ? 1 : 0)}_" +
                $"LC{(enableLethalCheck ? 1 : 0)}_" +
                $"AP{(enableHeuristicActionPruning ? 1 : 0)}_" +
                $"EV{(enableImprovedEvaluation ? 1 : 0)}";

            // 동적 탐색이 꺼져 있으면 MCTS가 사용하는 예산 표시
            if (UseTimeBudget)
            {
                label += $"_Time{TimeBudgetMilliseconds:F1}ms";
            }
            else if (UseFixedSimulationBudget)
            {
                label += $"_Budget{FixedSimulationBudget}";
            }

            // MCTSVanilla가 대결에 있으면 바닐라의 고정 반복 횟수 표시
            if (playerControllerType == ControllerType.MCTSVanilla ||
                enemyControllerType == ControllerType.MCTSVanilla)
            {
                label += $"_Vanilla{FixedSimulationBudget}";
            }

            return label;
        }
    }

    public int CurrentBattle => currentBattle;


    public static void RecordSearchTime(double ms)
    {
        totalSearchTimeMs += ms;
        searchCallCount++;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            totalSearchTimeMs = 0d;
            searchCallCount = 0;

            // 새 시뮬레이션 회차 시작
            MCTSLogger.StartNewRun(MCTSSearch.CurrentMode);
            MCTSVanillaLogger.StartNewRun(MCTSSearch.CurrentMode);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = timeScale;
    }

    /// <summary>
    /// 전투 종료 시 호출
    /// </summary>
    public void OnBattleEnded(bool playerWin)
    {
        // 자동 시뮬레이션이 아니면 아무것도 안 함
        if (!autoSimulation)
            return;

        // 승리 기록
        currentBattle++;

        // 승패 결과 CSV 기록
        MCTSLogger.LogBattleResult(
            currentBattle,
            MCTSSearch.CurrentMode,
            playerWin
        );

        if (playerWin)
        {
            playerWinCount++;
        }

        else
        {
            enemyWinCount++;
        }

        // 진행 상황 출력
        Debug.Log(
            $"[{currentBattle}/{totalBattleCount}] " +
            $"PlayerWin : {playerWinCount}, " +
            $"EnemyWin : {enemyWinCount}");

        // 시뮬레이션 종료
        if (currentBattle >= totalBattleCount)
        {
            PrintResult();

            return;
        }

        // 다음 판 시작
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 최종 결과 출력
    /// </summary>
    private void PrintResult()
    {
        Debug.Log("===== Simulation Result =====");

        Debug.Log(
            $"Player Win : {playerWinCount} " +
            $"({(float)playerWinCount / totalBattleCount * 100f:F2}%)");

        Debug.Log(
            $"Enemy Win : {enemyWinCount} " +
            $"({(float)enemyWinCount / totalBattleCount * 100f:F2}%)");

        Debug.Log(
            $"Average MCTS Search Time : {AverageSearchTimeMs:F2}ms " +
            $"(총 {searchCallCount}회 호출)");

        // 바닐라 MCTS가 대결에 있었으면 바닐라 평균 탐색 시간도 출력
        if (MCTSVanillaLogger.SearchCount > 0)
        {
            Debug.Log(
                $"Average Vanilla MCTS Search Time : {MCTSVanillaLogger.AverageSearchTimeMs:F2}ms " +
                $"(총 {MCTSVanillaLogger.SearchCount}회 호출)");
        }

        // 시뮬레이션 회차 종료 → 최종 파일명으로 변경
        MCTSLogger.FinishRun();
        MCTSVanillaLogger.FinishRun();
    }

    

    
}
