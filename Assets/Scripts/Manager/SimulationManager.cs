using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 실험 A에서 비교할 Vanilla MCTS의 보조 기능 구성입니다.
/// 휴리스틱 가지치기는 롤아웃 단계에만 적용하며, 트리 확장 단계의
/// 가지치기는 모든 조건에서 끈 상태로 유지합니다.
/// </summary>
public enum ExperimentACondition
{
    Vanilla,
    VanillaHeuristicPruning,
    VanillaLethalCheck,
    VanillaHeuristicPruningAndLethalCheck
}

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

    [Header("MCTS Optimization")]
    [SerializeField] private bool enableTranspositionCache = true;
    [SerializeField] private bool enableEarlyCutoff = true;
    [SerializeField][Range(5, 200)] private int earlyCutoffVisitThreshold = 30;
    [SerializeField] private bool enableHeuristicPruning = true;
    [SerializeField] private bool enableHeuristicActionPruning = true;
    [SerializeField] private bool enableLethalCheck = true;

    [Header("Experiment A - Search Budget")]
    [Tooltip("켜면 아래 조건으로 Vanilla MCTS의 휴리스틱 가지치기와 Lethal Check만 공정하게 비교합니다.")]
    [SerializeField] private bool enableExperimentA = true;
    [SerializeField] private ExperimentACondition experimentACondition = ExperimentACondition.Vanilla;
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

    public bool EnableTranspositionCache => enableTranspositionCache;
    public bool EnableEarlyCutoff => enableEarlyCutoff;
    public int EarlyCutoffVisitThreshold => earlyCutoffVisitThreshold;
    public bool EnableHeuristicRollout => enableHeuristicPruning;
    public bool EnableHeuristicActionPruning => enableHeuristicActionPruning;
    public bool EnableLethalCheck => enableLethalCheck;
    public bool UseFixedSimulationBudget =>
        enableExperimentA && searchBudgetMode == MCTSSearchBudgetMode.FixedSimulationCount;
    public bool UseTimeBudget =>
        enableExperimentA && searchBudgetMode == MCTSSearchBudgetMode.TimeLimit;
    public int FixedSimulationBudget => Mathf.Max(1, fixedSimulationBudget);
    public double TimeBudgetMilliseconds => Mathf.Max(0.1f, timeBudgetMilliseconds);
    public bool EnableSearchEarlyStop => !enableExperimentA;

    public string ExperimentModeLabel
    {
        get
        {
            if (!enableExperimentA)
                return "Standard_Dynamic";

            string budgetLabel = UseTimeBudget
                ? $"Time{TimeBudgetMilliseconds:F1}ms"
                : $"Budget{FixedSimulationBudget}";

            return $"ExperimentA_{experimentACondition}_{budgetLabel}";
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

            ApplyExperimentAConfiguration();
            totalSearchTimeMs = 0d;
            searchCallCount = 0;

            // 새 시뮬레이션 회차 시작
            MCTSLogger.StartNewRun(MCTSSearch.CurrentMode);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = timeScale;
    }

    /// <summary>
    /// 실험 A에서는 비교 대상 외의 최적화가 결과에 섞이지 않도록 끕니다.
    /// 휴리스틱 Action Pruning은 롤아웃 Heuristic Pruning과 별개이므로 항상 끕니다.
    /// </summary>
    private void ApplyExperimentAConfiguration()
    {
        if (!enableExperimentA)
            return;

        enableEarlyCutoff = false;
        enableHeuristicActionPruning = false;
        enableLethalCheck = false;

        switch (experimentACondition)
        {
            case ExperimentACondition.Vanilla:
                enableTranspositionCache = false;
                enableHeuristicPruning = false;
                enableLethalCheck = false;
                break;

            case ExperimentACondition.VanillaHeuristicPruning:
                enableTranspositionCache = false;
                enableHeuristicPruning = true;
                enableLethalCheck = false;
                break;

            case ExperimentACondition.VanillaLethalCheck:
                enableTranspositionCache = false;
                enableHeuristicPruning = false;
                enableLethalCheck = true;
                break;

            case ExperimentACondition.VanillaHeuristicPruningAndLethalCheck:
                enableTranspositionCache = false;
                enableHeuristicPruning = true;
                enableLethalCheck = true;
                break;
        }
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

        // 시뮬레이션 회차 종료 → 최종 파일명으로 변경
        MCTSLogger.FinishRun();
    }

    

    
}
