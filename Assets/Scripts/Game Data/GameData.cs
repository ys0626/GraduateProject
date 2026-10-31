using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;

    [Header("Player Start Setting")]
    [SerializeField] private int playerStartHP = 80;
    [SerializeField] private int playerStartGold = 99;
    [SerializeField] private int playerMaxEnergy = 3;
    [SerializeField] private CardData[] playerStartDeck;

    [Header("Normal Game - Enemy Pools")]
    [SerializeField] private EnemyData[] easyEnemyPool;
    [SerializeField] private EnemyData[] hardEnemyPool;
    [SerializeField] private EnemyData[] eliteEnemyPool;
    [SerializeField] private EnemyData[] bossEnemyPool;

    [Header("Normal Enemy Rewards")]
    [SerializeField] private RewardSettings normalEnemyReward;

    [Header("Elite Enemy Rewards")]
    [SerializeField] private RewardSettings eliteEnemyReward;

    [Header("Boss Enemy Rewards")]
    [SerializeField] private RewardSettings bossEnemyReward;

    [Header("Card Reward Pools")]
    [SerializeField] private CardData[] commonCardPool;
    [SerializeField] private CardData[] rareCardPool;

    [Header("Cheese Pool")]
    [SerializeField] private CheeseData[] cheesePool;

    private readonly CheeseData[] cheeseInventory = new CheeseData[3];

    public CheeseData[] CheesePool => cheesePool;
    public CheeseData[] CheeseInventory => cheeseInventory;

    public event Action OnCheeseInventoryChanged;

    [Header("Relic Pool")]
    [SerializeField] private List<RelicData> relicPool = new List<RelicData>();

    private BattleType currentBattleType = BattleType.Normal;

    // =========================================================
    // Enemy Selection History
    // =========================================================

    // 일반 전투 진행 횟수
    private int normalBattleCount;

    // Easy 적 등장 순서
    private readonly List<EnemyData> easyEnemyOrder =
        new List<EnemyData>();

    // Hard 적 마지막 등장 기록
    private EnemyData lastHardEnemy;

    // Elite 적 마지막 등장 기록
    private EnemyData lastEliteEnemy;

    // =========================================================
    // Runtime Entity
    // =========================================================

    public Entity player = new Entity();
    public Entity enemy = new Entity();

    // =========================================================
    // Current Enemy Data
    // =========================================================

    private EnemyData currentEnemyData;

    private string currentEnemyName;
    private Sprite currentEnemySprite;
    private RuntimeAnimatorController currentEnemyAnimatorController;

    // =========================================================
    // Player Start Setting Properties
    // =========================================================

    public int PlayerStartHP => playerStartHP;

    public int PlayerStartGold => playerStartGold;

    public int PlayerMaxEnergy => playerMaxEnergy;

    public CardData[] PlayerStartDeck => playerStartDeck;

    // =========================================================
    // Enemy Pool Properties
    // =========================================================

    public EnemyData[] EasyEnemyPool => easyEnemyPool;

    public EnemyData[] HardEnemyPool => hardEnemyPool;

    public EnemyData[] EliteEnemyPool => eliteEnemyPool;

    public EnemyData[] BossEnemyPool => bossEnemyPool;

    // =========================================================
    // Current Enemy Properties
    // =========================================================

    public EnemyData CurrentEnemyData => currentEnemyData;

    public string CurrentEnemyName => currentEnemyName;

    public Sprite CurrentEnemySprite => currentEnemySprite;

    public RuntimeAnimatorController CurrentEnemyAnimatorController
        => currentEnemyAnimatorController;

    // =========================================================
    // Reward Settings Properties
    // =========================================================

    public RewardSettings NormalEnemyReward => normalEnemyReward;

    public RewardSettings EliteEnemyReward => eliteEnemyReward;

    public RewardSettings BossEnemyReward => bossEnemyReward;

    public CardData[] CommonCardPool => commonCardPool;

    public CardData[] RareCardPool => rareCardPool;

    public IReadOnlyList<RelicData> RelicPool => relicPool;

    public BattleType CurrentBattleType
    {
        get => currentBattleType;
        set => currentBattleType = value;
    }

    // =========================================================
    // Singleton
    // =========================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // New Game
    // =========================================================

    public void InitNewGame()
    {
        ClearAllData();

        // Easy 적 등장 순서 생성
        PrepareEasyEnemyOrder();

        // Player HP
        player.MaxHP = playerStartHP;
        player.CurrentHP = playerStartHP;

        // Player Energy
        player.MaxEnergy = playerMaxEnergy;
        player.CurrentEnergy = playerMaxEnergy;

        // Player Gold
        player.Gold = playerStartGold;

        // Starting Deck
        if (playerStartDeck != null)
        {
            foreach (CardData cardData in playerStartDeck)
            {
                if (cardData == null)
                {
                    continue;
                }

                player.deck.Add(
                    new CardInstance(cardData)
                );
            }
        }

        UpdatePlayerPileCount();

        Debug.Log(
            $"[GameData] New Game Initialized - " +
            $"HP: {player.CurrentHP}, " +
            $"Gold: {player.Gold}, " +
            $"Deck: {player.deck.Count}"
        );
    }

    // =========================================================
    // Easy Enemy Order
    // =========================================================

    private void PrepareEasyEnemyOrder()
    {
        easyEnemyOrder.Clear();

        if (easyEnemyPool == null || easyEnemyPool.Length != 2)
        {
            Debug.LogError(
                "[GameData] Easy Enemy Pool must contain exactly 2 enemies."
            );

            return;
        }

        foreach (EnemyData enemyData in easyEnemyPool)
        {
            if (enemyData == null)
            {
                Debug.LogError(
                    "[GameData] Easy Enemy Pool contains null entries."
                );

                easyEnemyOrder.Clear();

                return;
            }

            // 같은 에셋이 중복 등록되었는지 확인
            if (easyEnemyOrder.Contains(enemyData))
            {
                Debug.LogError(
                    "[GameData] Easy Enemy Pool contains duplicate enemies."
                );

                easyEnemyOrder.Clear();

                return;
            }

            easyEnemyOrder.Add(enemyData);
        }

        // Fisher-Yates Shuffle
        for (int i = easyEnemyOrder.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            EnemyData temp = easyEnemyOrder[i];

            easyEnemyOrder[i] = easyEnemyOrder[randomIndex];

            easyEnemyOrder[randomIndex] = temp;
        }

        Debug.Log(
            "[GameData] Easy Enemy Order: " +
            string.Join(
                ", ",
                easyEnemyOrder.ConvertAll(
                    enemyData => enemyData.enemyName
                )
            )
        );
    }

    // =========================================================
    // Battle Initialization
    // =========================================================

    public void InitBattle()
    {
        // Player
        player.hand.Clear();
        player.drawPile.Clear();
        player.discardPile.Clear();
        player.exhaustPile.Clear();
        player.ClearStatuses();
        player.Block = 0;
        player.CurrentEnergy = player.MaxEnergy;

        // Enemy
        enemy.hand.Clear();
        enemy.drawPile.Clear();
        enemy.discardPile.Clear();
        enemy.exhaustPile.Clear();
        enemy.deck.Clear();
        enemy.ClearStatuses();
        enemy.Block = 0;

        UpdatePlayerPileCount();
        UpdateEnemyPileCount();
    }

    // =========================================================
    // Random Enemy Selection
    // =========================================================

    public EnemyData GetRandomEnemy()
    {
        return GetRandomEnemy(currentBattleType);
    }

    public EnemyData GetRandomEnemy(BattleType battleType)
    {
        EnemyData selectedEnemy = null;

        switch (battleType)
        {
            // -----------------------------------------
            // Normal Battle
            // -----------------------------------------

            case BattleType.Normal:

                // 첫 번째와 두 번째 일반 전투
                if (normalBattleCount < 2)
                {
                    if (easyEnemyOrder.Count != 2)
                    {
                        Debug.LogError(
                            "[GameData] Easy Enemy Order is not initialized."
                        );

                        return null;
                    }

                    selectedEnemy = easyEnemyOrder[normalBattleCount];

                    normalBattleCount++;
                }

                // 세 번째 일반 전투부터
                else
                {
                    selectedEnemy = GetRandomEnemyWithoutRepeat(
                        hardEnemyPool,
                        lastHardEnemy,
                        "Hard Enemy Pool"
                    );

                    if (selectedEnemy == null)
                    {
                        return null;
                    }

                    lastHardEnemy = selectedEnemy;

                    normalBattleCount++;
                }

                break;

            // -----------------------------------------
            // Elite Battle
            // -----------------------------------------

            case BattleType.Elite:

                selectedEnemy = GetRandomEnemyWithoutRepeat(
                    eliteEnemyPool,
                    lastEliteEnemy,
                    "Elite Enemy Pool"
                );

                if (selectedEnemy == null)
                {
                    return null;
                }

                lastEliteEnemy = selectedEnemy;

                break;

            // -----------------------------------------
            // Boss Battle
            // -----------------------------------------

            case BattleType.Boss:

                selectedEnemy = GetRandomEnemyFromPool(
                    bossEnemyPool,
                    "Boss Enemy Pool"
                );

                break;

            default:

                Debug.LogError(
                    $"[GameData] Unsupported BattleType: {battleType}"
                );

                return null;
        }

        if (selectedEnemy != null)
        {
            Debug.Log(
                $"[GameData] BattleType: {battleType}, " +
                $"Normal Battle Count: {normalBattleCount}, " +
                $"Selected Enemy: {selectedEnemy.enemyName}"
            );
        }

        return selectedEnemy;
    }

    // =========================================================
    // Random Enemy Without Consecutive Duplicate
    // =========================================================

    private EnemyData GetRandomEnemyWithoutRepeat(
        EnemyData[] selectedPool,
        EnemyData lastEnemy,
        string poolName)
    {
        if (selectedPool == null || selectedPool.Length == 0)
        {
            Debug.LogError(
                $"[GameData] {poolName} is empty."
            );

            return null;
        }

        List<EnemyData> validEnemies = new List<EnemyData>();

        foreach (EnemyData enemyData in selectedPool)
        {
            if (enemyData != null && !validEnemies.Contains(enemyData))
            {
                validEnemies.Add(enemyData);
            }
        }

        if (validEnemies.Count == 0)
        {
            Debug.LogError(
                $"[GameData] No valid EnemyData found in {poolName}."
            );

            return null;
        }

        List<EnemyData> availableEnemies = new List<EnemyData>();

        foreach (EnemyData enemyData in validEnemies)
        {
            if (enemyData != lastEnemy)
            {
                availableEnemies.Add(enemyData);
            }
        }

        // 대체할 적이 없는 경우
        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning(
                $"[GameData] {poolName} has no alternative enemy. " +
                "The same enemy may appear consecutively."
            );

            availableEnemies = validEnemies;
        }

        int index = UnityEngine.Random.Range(
            0,
            availableEnemies.Count
        );

        return availableEnemies[index];
    }

    // =========================================================
    // Random Enemy From Pool
    // =========================================================

    private EnemyData GetRandomEnemyFromPool(
        EnemyData[] selectedPool,
        string poolName)
    {
        if (selectedPool == null || selectedPool.Length == 0)
        {
            Debug.LogError(
                $"[GameData] {poolName} is empty."
            );

            return null;
        }

        List<EnemyData> validEnemies = new List<EnemyData>();

        foreach (EnemyData enemyData in selectedPool)
        {
            if (enemyData != null)
            {
                validEnemies.Add(enemyData);
            }
        }

        if (validEnemies.Count == 0)
        {
            Debug.LogError(
                $"[GameData] No valid EnemyData found in {poolName}."
            );

            return null;
        }

        int index = UnityEngine.Random.Range(
            0,
            validEnemies.Count
        );

        return validEnemies[index];
    }

    // =========================================================
    // Current Enemy
    // =========================================================

    public void SetCurrentEnemy(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError(
                "[GameData] EnemyData is null."
            );

            return;
        }

        currentEnemyData = enemyData;

        currentEnemyName = enemyData.enemyName;
        currentEnemySprite = enemyData.sprite;
        currentEnemyAnimatorController =
            enemyData.animatorController;

        Debug.Log(
            $"[GameData] Current Enemy Set: {currentEnemyName}"
        );
    }

    // =========================================================
    // Data Clear
    // =========================================================

    private void ClearAllData()
    {
        ClearEntity(player);
        ClearEntity(enemy);
        ClearCheeseInventory();

        RelicManager.EnsureInstance().ClearRunRelics();

        currentEnemyData = null;
        currentEnemyName = null;
        currentEnemySprite = null;
        currentEnemyAnimatorController = null;

        currentBattleType = BattleType.Normal;

        // Enemy Selection History 초기화
        normalBattleCount = 0;

        easyEnemyOrder.Clear();

        lastHardEnemy = null;

        lastEliteEnemy = null;
    }

    private void ClearEntity(Entity entity)
    {
        entity.hand.Clear();
        entity.drawPile.Clear();
        entity.discardPile.Clear();
        entity.exhaustPile.Clear();
        entity.deck.Clear();
        entity.ClearStatuses();
        entity.Block = 0;
    }

    // =========================================================
    // Pile Count
    // =========================================================

    public void UpdatePlayerPileCount()
    {
        player.DeckCount = player.deck.Count;

        player.DrawPileCount = player.drawPile.Count;

        player.DiscardPileCount = player.discardPile.Count;

        player.ExhaustPileCount = player.exhaustPile.Count;
    }

    public void UpdateEnemyPileCount()
    {
        enemy.DeckCount = enemy.deck.Count;

        enemy.DrawPileCount = enemy.drawPile.Count;

        enemy.DiscardPileCount = enemy.discardPile.Count;

        enemy.ExhaustPileCount = enemy.exhaustPile.Count;
    }

    // =========================================================
    // Cheese
    // =========================================================

    // 치즈를 왼쪽부터 빈 슬롯에 추가
    public bool AddCheese(CheeseData cheeseData)
    {
        if (cheeseData == null)
        {
            return false;
        }

        for (int i = 0; i < cheeseInventory.Length; i++)
        {
            if (cheeseInventory[i] == null)
            {
                cheeseInventory[i] = cheeseData;

                OnCheeseInventoryChanged?.Invoke();

                return true;
            }
        }

        return false;
    }

    // 지정한 슬롯의 치즈 사용
    public bool UseCheese(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= cheeseInventory.Length)
        {
            return false;
        }

        CheeseData cheese = cheeseInventory[slotIndex];

        if (cheese == null || player == null)
        {
            return false;
        }

        if (cheese.IsRotten)
        {
            // 최대 HP를 먼저 올린 뒤 현재 HP를 회복한다.
            player.MaxHP += cheese.effectAmount;
            player.Heal(cheese.effectAmount);
        }
        else
        {
            player.Heal(cheese.effectAmount);
        }

        cheeseInventory[slotIndex] = null;

        OnCheeseInventoryChanged?.Invoke();

        return true;
    }

    // 새 게임 시작 시 인벤토리 초기화
    private void ClearCheeseInventory()
    {
        for (int i = 0; i < cheeseInventory.Length; i++)
        {
            cheeseInventory[i] = null;
        }

        OnCheeseInventoryChanged?.Invoke();
    }
}