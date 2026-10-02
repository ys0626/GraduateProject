using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private bool newGameInitialized;

    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (instance != null)
        {
            CleanUpAndDestroy();
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        MarkPersistentObjects();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitNewGame();
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =========================================================
    // New Game
    // =========================================================

    private void InitNewGame()
    {
        if (newGameInitialized)
            return;

        if (GameData.instance == null)
        {
            Debug.LogError(
                "[GameManager] GameData가 없습니다."
            );

            return;
        }

        GameData.instance.InitNewGame();

        newGameInitialized = true;

        Debug.Log(
            "[GameManager] New Game Started"
        );
    }

    // =========================================================
    // Scene Loaded
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (scene.name != battleSceneName)
            return;

        InitBattleScene();
    }

    // =========================================================
    // Battle Scene
    // =========================================================

    private void InitBattleScene()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[GameManager] BattleScene에 " +
                "GameData가 없습니다."
            );

            return;
        }

        SimulationManager simulationManager =
            SimulationManager.instance;

        if (simulationManager == null)
        {
            Debug.LogError(
                "[GameManager] BattleScene에 " +
                "SimulationManager가 없습니다."
            );

            return;
        }

        if (simulationManager.AutoSimulation)
        {
            InitSimulationBattle(
                simulationManager
            );

            return;
        }

        InitNormalBattle();
    }

    // =========================================================
    // Normal Game Battle
    // =========================================================

    private void InitNormalBattle()
    {
        GameData data =
            GameData.instance;

        data.InitBattle();

        // -----------------------------------------
        // Select Enemy
        // -----------------------------------------

        EnemyData enemyData =
            data.GetRandomEnemy();

        if (enemyData == null)
        {
            Debug.LogError(
                "[GameManager] 일반 게임용 EnemyData를 " +
                "선택하지 못했습니다."
            );

            return;
        }

        // -----------------------------------------
        // Enemy Deck
        // -----------------------------------------

        if (enemyData.starterDeck != null)
        {
            foreach (
                CardData cardData
                in enemyData.starterDeck)
            {
                if (cardData == null)
                    continue;

                data.enemy.deck.Add(
                    new CardInstance(cardData)
                );
            }
        }

        // -----------------------------------------
        // Enemy Stats
        // -----------------------------------------

        data.enemy.MaxHP =
            enemyData.maxHP;

        data.enemy.CurrentHP =
            enemyData.maxHP;

        data.enemy.MaxEnergy =
            enemyData.maxEnergy;

        data.enemy.CurrentEnergy =
            enemyData.maxEnergy;

        // -----------------------------------------
        // Current Enemy Visual Data
        // -----------------------------------------

        data.SetCurrentEnemy(
            enemyData
        );

        // -----------------------------------------
        // Initialize Decks
        // -----------------------------------------

        PlayerDeckManager.instance
            .InitPlayerDeck();

        EnemyDeckManager.instance
            .InitEnemyDeck();

        data.UpdatePlayerPileCount();
        data.UpdateEnemyPileCount();

        Debug.Log(
            $"[GameManager] Normal Battle Initialized - " +
            $"Enemy : {enemyData.enemyName}, " +
            $"Player HP : {data.player.CurrentHP}, " +
            $"Player Gold : {data.player.Gold}, " +
            $"Enemy HP : {data.enemy.CurrentHP}"
        );

        InitBattleManager();
    }

    // =========================================================
    // Simulation Battle
    // =========================================================

    private void InitSimulationBattle(
        SimulationManager simulationManager)
    {
        GameData data =
            GameData.instance;

        data.InitBattle();

        // -----------------------------------------
        // Simulation Player
        // -----------------------------------------

        if (simulationManager.SimulationPlayerDeck != null)
        {
            foreach (
                CardData cardData
                in simulationManager.SimulationPlayerDeck)
            {
                if (cardData == null)
                    continue;

                data.player.deck.Add(
                    new CardInstance(cardData)
                );
            }
        }

        data.player.MaxHP =
            simulationManager.SimulationPlayerMaxHP;

        data.player.CurrentHP =
            simulationManager.SimulationPlayerMaxHP;

        data.player.MaxEnergy =
            simulationManager.SimulationPlayerMaxEnergy;

        data.player.CurrentEnergy =
            simulationManager.SimulationPlayerMaxEnergy;

        // -----------------------------------------
        // Simulation Enemy
        // -----------------------------------------

        if (simulationManager.SimulationEnemyDeck != null)
        {
            foreach (
                CardData cardData
                in simulationManager.SimulationEnemyDeck)
            {
                if (cardData == null)
                    continue;

                data.enemy.deck.Add(
                    new CardInstance(cardData)
                );
            }
        }

        data.enemy.MaxHP =
            simulationManager.SimulationEnemyMaxHP;

        data.enemy.CurrentHP =
            simulationManager.SimulationEnemyMaxHP;

        data.enemy.MaxEnergy =
            simulationManager.SimulationEnemyMaxEnergy;

        data.enemy.CurrentEnergy =
            simulationManager.SimulationEnemyMaxEnergy;

        // -----------------------------------------
        // Initialize Decks
        // -----------------------------------------

        PlayerDeckManager.instance
            .InitPlayerDeck();

        EnemyDeckManager.instance
            .InitEnemyDeck();

        data.UpdatePlayerPileCount();
        data.UpdateEnemyPileCount();

        Debug.Log(
            $"[GameManager] Simulation Battle Initialized - " +
            $"Player HP : {data.player.CurrentHP}, " +
            $"Enemy HP : {data.enemy.CurrentHP}"
        );

        InitBattleManager();
    }

    // =========================================================
    // BattleManager
    // =========================================================

    private void InitBattleManager()
    {
        if (BattleManager.instance == null)
        {
            Debug.LogError(
                "[GameManager] BattleManager가 없습니다."
            );

            return;
        }

        BattleManager.instance.Init();

        SimulationManager simulationManager =
            SimulationManager.instance;

        if (simulationManager == null ||
            !simulationManager.AutoSimulation)
        {
            if (BattleUIManager.instance != null)
            {
                BattleUIManager.instance.UpdateAll();
            }
        }
    }

    // =========================================================
    // Persistent Objects
    // =========================================================

    private void MarkPersistentObjects()
    {
        if (persistentObjects == null)
            return;

        foreach (GameObject obj in persistentObjects)
        {
            if (obj == null)
                continue;

            DontDestroyOnLoad(obj);
        }
    }

    private void CleanUpAndDestroy()
    {
        if (persistentObjects != null)
        {
            foreach (GameObject obj in persistentObjects)
            {
                if (obj == null)
                    continue;

                Destroy(obj);
            }
        }

        Destroy(gameObject);
    }
}