using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase
{
    Idle,           // ´ë±â
    BattleStart,    // ÀüÅõ ½ÃÀÛ
    PlayerTurn,     // ÇÃ·¹ÀÌ¾î ÅÏ
    EnemyTurn,      // Àû ÅÏ
    TurnEnd,        // ÅÏ Á¾·á Ã³¸®
    BattleEnd       // ÀüÅõ Á¾·á
}

public enum BattleMode { Normal, Hard } // ³ë¸», ÇÏµå ¸ðµå ±¸ºÐ

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Mode")]
    [SerializeField] private BattleMode battleMode;

    [Header("Managers")]
    [SerializeField] private GameStateManager gameStateManager; // GameState °ü·Ã ·ÎÁ÷ ´ã´ç
    [SerializeField] private BattleUIManager uiManager; // UI ¾÷µ¥ÀÌÆ® ´ã´ç
    [SerializeField] private EnemyAIController enemyAIController; // ³ë¸» ¸ðµå Àû
    [SerializeField] private MCTSController mctsController; // MCTS ¾Ë°í¸®Áò ´ã´ç

    [Header("Database")]
    [SerializeField] private CardDatabase cardDatabase;

    // ¦¡¦¡¦¡ »óÅÂ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public GameState CurrentState { get; private set; }
    public BattlePhase CurrentPhase { get; private set; }

    // ¦¡¦¡¦¡ ÀÌº¥Æ® ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public event Action<BattlePhase> OnPhaseChanged;
    public event Action<GameState> OnStateUpdated;
    public event Action<bool> OnBattleEnd;  // true = ÇÃ·¹ÀÌ¾î ½Â¸®

    // ¦¡¦¡¦¡ ¼³Á¤ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("Battle Settings")]
    [SerializeField] private int drawCount = 5;
    [SerializeField] private float enemyActionDelay = 1.0f;  // Àû Çàµ¿ µô·¹ÀÌ

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀüÅõ ½ÃÀÛ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void StartBattle(Entity player, Entity enemy, List<CardDatabase.CardEntry> playerDeck)
    {
        // ÃÊ±â GameState ±¸¼º
        CurrentState = new GameState
        {
            player = player,
            enemy = enemy,
            deck = playerDeck,
            hand = new List<CardDatabase.CardEntry>(),
            discardPile = new List<CardDatabase.CardEntry>(),
            currentEnergy = 3,
            maxEnergy = 3,
            turnCount = 0,
            isPlayerTurn = true
        };

        // µ¦ ¼ÅÇÃ
        gameStateManager.ShuffleDeck(CurrentState.deck);

        ChangePhase(BattlePhase.BattleStart);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÆäÀÌÁî ÀüÈ¯ (ÇÙ½É Èå¸§ Á¦¾î)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void ChangePhase(BattlePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        switch (newPhase)
        {
            case BattlePhase.BattleStart:
                HandleBattleStart();
                break;
            case BattlePhase.PlayerTurn:
                HandlePlayerTurnStart();
                break;
            case BattlePhase.EnemyTurn:
                HandleEnemyTurn();
                break;
            case BattlePhase.TurnEnd:
                HandleTurnEnd();
                break;
            case BattlePhase.BattleEnd:
                HandleBattleEnd();
                break;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀüÅõ ½ÃÀÛ Ã³¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void HandleBattleStart()
    {
        Debug.Log("ÀüÅõ ½ÃÀÛ!");
        // ÃÊ±â µå·Î¿ì
        CurrentState = gameStateManager.DrawCards(CurrentState, drawCount);
        UpdateUI();

        if (battleMode == BattleMode.Normal) // ³ë¸» ¸ðµå¿¡¼­´Â Ã¹ ÅÏºÎÅÍ Àû Çàµ¿ ¹Ì¸® º¸¿©ÁÖ±â
        {
            enemyAIController.PrepareNextAction(CurrentState);
        }

        ChangePhase(BattlePhase.PlayerTurn);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÃ·¹ÀÌ¾î ÅÏ ½ÃÀÛ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void HandlePlayerTurnStart()
    {
        Debug.Log($"[ÅÏ {CurrentState.turnCount}] ÇÃ·¹ÀÌ¾î ÅÏ ½ÃÀÛ");

        // ¿¡³ÊÁö ÃæÀü
        CurrentState.currentEnergy = CurrentState.maxEnergy;

        // ¹æ¾îµµ ÃÊ±âÈ­
        CurrentState.player.block = 0;

        if (battleMode == BattleMode.Normal) // ³ë¸» ¸ðµå¿¡¼­´Â ´ÙÀ½ Àû Çàµ¿ ¹Ì¸® º¸¿©ÁÖ±â
        {
            uiManager.ShowEnemyIntent(enemyAIController.NextIntent);
        }

        UpdateUI();
        // ÀÌÈÄ ÇÃ·¹ÀÌ¾î ÀÔ·Â ´ë±â (UI¿¡¼­ Ä«µå Å¬¸¯)
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÃ·¹ÀÌ¾î Ä«µå »ç¿ë (UI¿¡¼­ È£Ãâ)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void PlayerUseCard(CardDatabase.CardEntry card)
    {
        // À¯È¿¼º °Ë»ç
        if (CurrentPhase != BattlePhase.PlayerTurn)
        {
            Debug.LogWarning("ÇÃ·¹ÀÌ¾î ÅÏÀÌ ¾Æ´Õ´Ï´Ù.");
            return;
        }

        if (card.cost > CurrentState.currentEnergy)
        {
            Debug.LogWarning("¿¡³ÊÁö°¡ ºÎÁ·ÇÕ´Ï´Ù.");
            uiManager.ShowNotEnoughEnergy();
            return;
        }

        Debug.Log($"ÇÃ·¹ÀÌ¾î°¡ [{card.cardName}] »ç¿ë");

        // »óÅÂ ¾÷µ¥ÀÌÆ®
        CurrentState = gameStateManager.ApplyAction(CurrentState, card);
        OnStateUpdated?.Invoke(CurrentState);
        UpdateUI();

        // ÀüÅõ Á¾·á Ã¼Å©
        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            return;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÃ·¹ÀÌ¾î ÅÏ Á¾·á (¹öÆ°¿¡¼­ È£Ãâ)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void PlayerEndTurn()
    {
        if (CurrentPhase != BattlePhase.PlayerTurn) return;

        Debug.Log("ÇÃ·¹ÀÌ¾î ÅÏ Á¾·á");
        ChangePhase(BattlePhase.EnemyTurn);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Àû ÅÏ Ã³¸® (MCTS È£Ãâ)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void HandleEnemyTurn()
    {
        Debug.Log("Àû ÅÏ ½ÃÀÛ");

        // Àû ¹æ¾îµµ ÃÊ±âÈ­
        CurrentState.enemy.block = 0;

        StartCoroutine(battleMode == BattleMode.Normal
            ? NormalEnemyTurnCoroutine()
            : HardEnemyTurnCoroutine());
    }

    // ³ë¸» ¸ðµå: ÆÐÅÏ AI
    private IEnumerator NormalEnemyTurnCoroutine()
    {
        uiManager.ShowEnemyAction(enemyAIController.NextIntent); // Àû Çàµ¿ ¿¬Ãâ?
        yield return new WaitForSeconds(enemyActionDelay);

        // Çàµ¿ ½ÇÇà
        CurrentState = enemyAIController.ExecuteAction(CurrentState);
        OnStateUpdated?.Invoke(CurrentState);
        UpdateUI();

        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            yield break;
        }

        ChangePhase(BattlePhase.TurnEnd);
    }


    private IEnumerator HardEnemyTurnCoroutine()
    {
        uiManager.ShowEnemyThinking();  // "»ý°¢ Áß..." °°Àº UI

        // MCTS ½ÇÇà (ºñµ¿±â)
        CardDatabase.CardEntry bestCard = null;
        bool isDone = false;

        mctsController.GetBestAction(
            CurrentState,
            result => {
                bestCard = result;
                isDone = true;
            }
        );

        // MCTS ¿Ï·á ´ë±â
        yield return new WaitUntil(() => isDone);
        yield return new WaitForSeconds(enemyActionDelay);  // ¿¬Ãâ¿ë µô·¹ÀÌ

        uiManager.HideEnemyThinking();

        // »ç¿ëÇÒ Ä«µå°¡ ¾øÀ¸¸é ÅÏ Á¾·á
        if (bestCard == null)
        {
            Debug.Log("Àû: »ç¿ëÇÒ Ä«µå ¾øÀ½, ÅÏ Á¾·á");
            ChangePhase(BattlePhase.TurnEnd);
            yield break;
        }

        // Àû Ä«µå »ç¿ë
        Debug.Log($"ÀûÀÌ [{bestCard.cardName}] »ç¿ë");
        uiManager.ShowEnemyAction(bestCard);  // Àû Çàµ¿ ¿¬Ãâ

        yield return new WaitForSeconds(0.5f);

        CurrentState = gameStateManager.ApplyAction(CurrentState, bestCard);
        OnStateUpdated?.Invoke(CurrentState);
        UpdateUI();

        // ÀüÅõ Á¾·á Ã¼Å©
        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            yield break;
        }

        ChangePhase(BattlePhase.TurnEnd);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÅÏ Á¾·á Ã³¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void HandleTurnEnd()
    {
        // »óÅÂÀÌ»ó Ã³¸® (µ¶, È­»ó µî)
        ApplyStatusEffects();

        if (CurrentState.IsTerminal()) // »óÅÂÀÌ»óÀ¸·Î ÀÎÇØ ÀüÅõ Á¾·áµÉ ¼ö ÀÖÀ½
        {
            ChangePhase(BattlePhase.BattleEnd);
            return;
        }

        // µå·Î¿ì
        CurrentState = gameStateManager.DrawCards(CurrentState, drawCount);
        CurrentState.turnCount++;

        UpdateUI();
        ChangePhase(BattlePhase.PlayerTurn);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // »óÅÂÀÌ»ó Ã³¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void ApplyStatusEffects()
    {
        ApplyBurnEffect(CurrentState.player);
        ApplyBurnEffect(CurrentState.enemy);

        // µ¶ÀÌ¶û È­»ó »óÅÂÀÌ»ó °í¹Î Áß, µÑÀÌ ºñ½ÁÇÑ È¿°ú¶ó¸é ±×³É ÇÏ³ª¸¸ ÇÏ´Â °Ô ³´Áö ¾ÊÀ»±î ½ÍÀ½
    }

    private void ApplyBurnEffect(Entity entity)
    {
        if (entity.statusEffects.TryGetValue(EffectType.Burn, out int burnValue))
        {
            entity.TakeDamage(burnValue);
            // ÅÏ¸¶´Ù °¨¼Ò½ÃÅ°·Á¸é ¾Æ·¡ ÁÖ¼® ÇØÁ¦
            // entity.statusEffects[EffectType.Burn]--;
            // if (entity.statusEffects[EffectType.Burn] <= 0)
            //     entity.statusEffects.Remove(EffectType.Burn);
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀüÅõ Á¾·á
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void HandleBattleEnd()
    {
        bool playerWin = CurrentState.enemy.currentHP <= 0;
        Debug.Log(playerWin ? "ÇÃ·¹ÀÌ¾î ½Â¸®!" : "ÇÃ·¹ÀÌ¾î ÆÐ¹è...");

        StopAllCoroutines(); // È¤½Ã ³²¾ÆÀÖ´Â Àû Çàµ¿ ÄÚ·çÆ¾ÀÌ ÀÖ´Ù¸é ÁßÁö

        OnBattleEnd?.Invoke(playerWin);
        uiManager.ShowBattleResult(playerWin);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // UI ¾÷µ¥ÀÌÆ®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void UpdateUI()
    {
        OnStateUpdated?.Invoke(CurrentState);
        uiManager.UpdateAll(CurrentState);
    }
}