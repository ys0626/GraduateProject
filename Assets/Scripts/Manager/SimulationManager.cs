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

    private int currentBattle;

    private int playerWinCount;
    private int enemyWinCount;

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

    public int CurrentBattle
        => currentBattle;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

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

        // 승패 결과 CSV 기록
        MCTSLogger.LogBattleResult(
            currentBattle,
            MCTSSearch.CurrentMode,
            playerWin
        );

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

        // 시뮬레이션 회차 종료 → 최종 파일명으로 변경
        MCTSLogger.FinishRun();
    }
}