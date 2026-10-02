using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance;

    [Header("Auto Simulation - Player Setting")]
    [SerializeField] private CardData[] simulationPlayerDeck;
    [SerializeField] private int simulationPlayerMaxHP = 80;
    [SerializeField] private int simulationPlayerMaxEnergy = 3;

    [Header("Auto Simulation - Enemy Setting")]
    [SerializeField] private CardData[] simulationEnemyDeck;
    [SerializeField] private int simulationEnemyMaxHP = 80;
    [SerializeField] private int simulationEnemyMaxEnergy = 3;

    [Header("Controller Setting")]
    [SerializeField] private ControllerType playerControllerType;
    [SerializeField] private ControllerType enemyControllerType;

    [Header("Simulation")]
    [SerializeField] private bool autoSimulation;
    [SerializeField] private int totalBattleCount = 100;

    [Header("Speed")]
    [SerializeField] private float timeScale = 1f;

    [Header("MCTS Optimization")]
    [SerializeField] private bool enableTranspositionCache = true;
    [SerializeField] private bool enableEarlyCutoff = true;

    [SerializeField]
    [Range(5, 200)]
    private int earlyCutoffVisitThreshold = 30;

    [SerializeField] private bool enableHeuristicPruning = true;
    [SerializeField] private bool enableLethalCheck = true;

    // =========================================================
    // Static Simulation Data
    // =========================================================

    private static int currentBattle;
    private static int playerWinCount;
    private static int enemyWinCount;

    private static float totalSearchTimeMs;
    private static int searchCallCount;

    private static bool simulationStarted;

    // =========================================================
    // Properties
    // =========================================================

    public bool AutoSimulation =>
        autoSimulation;

    public CardData[] SimulationPlayerDeck =>
        simulationPlayerDeck;

    public int SimulationPlayerMaxHP =>
        simulationPlayerMaxHP;

    public int SimulationPlayerMaxEnergy =>
        simulationPlayerMaxEnergy;

    public CardData[] SimulationEnemyDeck =>
        simulationEnemyDeck;

    public int SimulationEnemyMaxHP =>
        simulationEnemyMaxHP;

    public int SimulationEnemyMaxEnergy =>
        simulationEnemyMaxEnergy;

    public ControllerType PlayerControllerType =>
        playerControllerType;

    public ControllerType EnemyControllerType =>
        enemyControllerType;

    public bool EnableTranspositionCache =>
        enableTranspositionCache;

    public bool EnableEarlyCutoff =>
        enableEarlyCutoff;

    public int EarlyCutoffVisitThreshold =>
        earlyCutoffVisitThreshold;

    public bool EnableHeuristicPruning =>
        enableHeuristicPruning;

    public bool EnableLethalCheck =>
        enableLethalCheck;

    public int CurrentBattle =>
        currentBattle;

    public static float AverageSearchTimeMs =>
        searchCallCount > 0
            ? totalSearchTimeMs / searchCallCount
            : 0f;

    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        Time.timeScale = timeScale;

        if (autoSimulation &&
            !simulationStarted)
        {
            StartSimulationSession();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // =========================================================
    // Simulation Start
    // =========================================================

    private void StartSimulationSession()
    {
        simulationStarted = true;

        currentBattle = 0;
        playerWinCount = 0;
        enemyWinCount = 0;

        totalSearchTimeMs = 0f;
        searchCallCount = 0;

        MCTSLogger.StartNewRun(
            MCTSSearch.CurrentMode
        );

        Debug.Log(
            $"[Simulation] Started - " +
            $"Total Battles : {totalBattleCount}"
        );
    }

    // =========================================================
    // MCTS Search Time
    // =========================================================

    public static void RecordSearchTime(float ms)
    {
        totalSearchTimeMs += ms;
        searchCallCount++;
    }

    // =========================================================
    // Battle End
    // =========================================================

    public void OnBattleEnded(bool playerWin)
    {
        if (!autoSimulation)
            return;

        currentBattle++;

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

        Debug.Log(
            $"[{currentBattle}/{totalBattleCount}] " +
            $"PlayerWin : {playerWinCount}, " +
            $"EnemyWin : {enemyWinCount}"
        );

        if (currentBattle >= totalBattleCount)
        {
            PrintResult();

            simulationStarted = false;

            return;
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // =========================================================
    // Result
    // =========================================================

    private void PrintResult()
    {
        Debug.Log(
            "===== Simulation Result ====="
        );

        if (totalBattleCount > 0)
        {
            Debug.Log(
                $"Player Win : {playerWinCount} " +
                $"({(float)playerWinCount / totalBattleCount * 100f:F2}%)"
            );

            Debug.Log(
                $"Enemy Win : {enemyWinCount} " +
                $"({(float)enemyWinCount / totalBattleCount * 100f:F2}%)"
            );
        }

        Debug.Log(
            $"Average MCTS Search Time : " +
            $"{AverageSearchTimeMs:F2}ms " +
            $"(총 {searchCallCount}회 호출)"
        );

        MCTSLogger.FinishRun();
    }
}