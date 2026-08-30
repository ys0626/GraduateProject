using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private bool enableLethalCheck = true;

    private int currentBattle;

    private int playerWinCount;
    private int enemyWinCount;

    private static float totalSearchTimeMs;
    private static int searchCallCount;
    public static float AverageSearchTimeMs =>
    searchCallCount > 0 ? totalSearchTimeMs / searchCallCount : 0f;

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
    public bool EnableHeuristicPruning => enableHeuristicPruning;
    public bool EnableLethalCheck => enableLethalCheck;


    public static void RecordSearchTime(float ms)
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
    }

    

    
}