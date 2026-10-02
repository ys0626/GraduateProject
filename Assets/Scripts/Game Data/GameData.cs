using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData instance;

    [Header("Player Start Setting")]
    [SerializeField] private int playerStartHP = 80;
    [SerializeField] private int playerStartGold = 99;
    [SerializeField] private int playerMaxEnergy = 3;
    [SerializeField] private CardData[] playerStartDeck;

    [Header("Normal Game - Enemy Pool")]
    [SerializeField] private EnemyData[] enemyDatas;

    public Entity player = new Entity();
    public Entity enemy = new Entity();

    private string currentEnemyName;
    private Sprite currentEnemySprite;
    private RuntimeAnimatorController currentEnemyAnimatorController;

    public int PlayerStartHP => playerStartHP;
    public int PlayerStartGold => playerStartGold;
    public int PlayerMaxEnergy => playerMaxEnergy;

    public CardData[] PlayerStartDeck =>
        playerStartDeck;

    public string CurrentEnemyName =>
        currentEnemyName;

    public Sprite CurrentEnemySprite =>
        currentEnemySprite;

    public RuntimeAnimatorController CurrentEnemyAnimatorController =>
        currentEnemyAnimatorController;

    private void Awake()
    {
        if (instance != null)
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

        // Player
        player.MaxHP = playerStartHP;
        player.CurrentHP = playerStartHP;

        player.MaxEnergy = playerMaxEnergy;
        player.CurrentEnergy = playerMaxEnergy;

        player.Gold = playerStartGold;

        // Starting Deck
        if (playerStartDeck != null)
        {
            foreach (CardData cardData in playerStartDeck)
            {
                if (cardData == null)
                    continue;

                player.deck.Add(
                    new CardInstance(cardData)
                );
            }
        }

        UpdatePlayerPileCount();

        Debug.Log(
            $"[GameData] New Game Initialized - " +
            $"HP : {player.CurrentHP}, " +
            $"Gold : {player.Gold}, " +
            $"Deck : {player.deck.Count}"
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

        player.Block = 0;
        player.CurrentEnergy = player.MaxEnergy;

        // Enemy
        enemy.hand.Clear();
        enemy.drawPile.Clear();
        enemy.discardPile.Clear();
        enemy.exhaustPile.Clear();
        enemy.deck.Clear();

        enemy.Block = 0;

        UpdatePlayerPileCount();
        UpdateEnemyPileCount();
    }

    // =========================================================
    // Enemy Data
    // =========================================================

    public EnemyData GetRandomEnemy()
    {
        if (enemyDatas == null ||
            enemyDatas.Length == 0)
        {
            Debug.LogError(
                "[GameData] EnemyData가 없습니다."
            );

            return null;
        }

        int index = Random.Range(
            0,
            enemyDatas.Length
        );

        EnemyData selectedEnemy =
            enemyDatas[index];

        Debug.Log(
            $"[GameData] Selected Enemy : " +
            $"{selectedEnemy.enemyName}"
        );

        return selectedEnemy;
    }

    // =========================================================
    // Current Enemy
    // =========================================================

    public void SetCurrentEnemy(
        EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError(
                "[GameData] EnemyData가 null입니다."
            );

            return;
        }

        currentEnemyName =
            enemyData.enemyName;

        currentEnemySprite =
            enemyData.sprite;

        currentEnemyAnimatorController =
            enemyData.animatorController;
    }

    // =========================================================
    // Data Clear
    // =========================================================

    private void ClearAllData()
    {
        ClearEntity(player);
        ClearEntity(enemy);

        currentEnemyName = null;
        currentEnemySprite = null;
        currentEnemyAnimatorController = null;
    }

    private void ClearEntity(Entity entity)
    {
        entity.hand.Clear();
        entity.drawPile.Clear();
        entity.discardPile.Clear();
        entity.exhaustPile.Clear();
        entity.deck.Clear();

        entity.Block = 0;
    }

    // =========================================================
    // Pile Count
    // =========================================================

    public void UpdatePlayerPileCount()
    {
        player.DeckCount =
            player.deck.Count;

        player.DrawPileCount =
            player.drawPile.Count;

        player.DiscardPileCount =
            player.discardPile.Count;

        player.ExhaustPileCount =
            player.exhaustPile.Count;
    }

    public void UpdateEnemyPileCount()
    {
        enemy.DeckCount =
            enemy.deck.Count;

        enemy.DrawPileCount =
            enemy.drawPile.Count;

        enemy.DiscardPileCount =
            enemy.discardPile.Count;

        enemy.ExhaustPileCount =
            enemy.exhaustPile.Count;
    }
}