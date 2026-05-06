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

public enum BattleMode { Normal, Hard }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Mode")]
    [SerializeField] private BattleMode battleMode;

    [Header("Managers")]
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private MCTSController mctsController;

    // ¦¡¦¡¦¡ »óÅÂ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public GameState CurrentState { get; private set; }
    public BattlePhase CurrentPhase { get; private set; }

    // ¦¡¦¡¦¡ ÀÌº¥Æ® ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public event Action<BattlePhase> OnPhaseChanged;
    public event Action<GameState> OnStateUpdated;
    public event Action<bool> OnBattleEnd;   // true = ÇÃ·¹ÀÌ¾î ½Â¸®

    // ¦¡¦¡¦¡ ¼³Á¤ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("Battle Settings")]
    [SerializeField] private int drawCount = 5;
    [SerializeField] private float enemyActionDelay = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀüÅõ ½ÃÀÛ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void StartBattle(Entity player, Entity enemy, List<CardInstance> playerDeck)
    {
        CurrentState = new GameState
        {
            player = player,
            enemy = enemy,
            deck = new List<CardInstance>(playerDeck),
            hand = new List<CardInstance>(),
            discardPile = new List<CardInstance>(),
            exhaustPile = new List<CardInstance>(),
            // DBTest °ª »ç¿ë (ÇÏµåÄÚµù Á¦°Å)
            currentEnergy = DBTest.instance.MaxEnergy,
            maxEnergy = DBTest.instance.MaxEnergy,
            turnCount = 0,
            isPlayerTurn = true
        };

        gameStateManager.ShuffleDeck(CurrentState.deck);
        ChangePhase(BattlePhase.BattleStart);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÆäÀÌÁî ÀüÈ¯
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void ChangePhase(BattlePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        switch (newPhase)
        {
            case BattlePhase.BattleStart: HandleBattleStart(); break;
            case BattlePhase.PlayerTurn: HandlePlayerTurnStart(); break;
            case BattlePhase.EnemyTurn: HandleEnemyTurn(); break;
            case BattlePhase.TurnEnd: HandleTurnEnd(); break;
            case BattlePhase.BattleEnd: HandleBattleEnd(); break;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀüÅõ ½ÃÀÛ Ã³¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void HandleBattleStart()
    {
        Debug.Log("ÀüÅõ ½ÃÀÛ!");

        // Ã¹ µå·Î¿ì´Â BattleStart¿¡¼­¸¸
        CurrentState = gameStateManager.DrawCards(CurrentState, drawCount);
        UpdateUI();

        if (battleMode == BattleMode.Normal)
            enemyController.PrepareNextAction(CurrentState);

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

        if (battleMode == BattleMode.Normal)
            uiManager.ShowEnemyIntent(enemyController.NextIntent);

        UpdateUI();
        // ÀÌÈÄ ÇÃ·¹ÀÌ¾î ÀÔ·Â ´ë±â
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÃ·¹ÀÌ¾î Ä«µå »ç¿ë (UI¿¡¼­ È£Ãâ)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void PlayerUseCard(CardInstance card)
    {
        if (CurrentPhase != BattlePhase.PlayerTurn)
        {
            Debug.LogWarning("ÇÃ·¹ÀÌ¾î ÅÏÀÌ ¾Æ´Õ´Ï´Ù.");
            return;
        }

        if (card.data.cost > CurrentState.currentEnergy)
        {
            Debug.LogWarning("¿¡³ÊÁö°¡ ºÎÁ·ÇÕ´Ï´Ù.");
            uiManager.ShowEnergyWarning();
            return;
        }

        Debug.Log($"ÇÃ·¹ÀÌ¾î°¡ [{card.data.cardName}] »ç¿ë");

        CurrentState = gameStateManager.ApplyAction(CurrentState, card);
        UpdateUI();

        if (CurrentState.IsTerminal())
            ChangePhase(BattlePhase.BattleEnd);
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
    // Àû ÅÏ Ã³¸®
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
        uiManager.ShowEnemyAction(enemyController.NextIntent);
        yield return new WaitForSeconds(enemyActionDelay);

        CurrentState = enemyController.ExecuteAction(CurrentState);
        UpdateUI(); // OnStateUpdated Áßº¹ È£Ãâ Á¦°Å (UpdateUI ³»ºÎ¿¡¼­ Ã³¸®)

        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            yield break;
        }

        ChangePhase(BattlePhase.TurnEnd);
    }

    // ÇÏµå ¸ðµå: MCTS AI
    private IEnumerator HardEnemyTurnCoroutine()
    {
        CardInstance bestCard = null;
        bool isDone = false;

        mctsController.GetBestAction(
            CurrentState,
            result =>
            {
                bestCard = result;
                isDone = true;
            }
        );

        yield return new WaitUntil(() => isDone);
        yield return new WaitForSeconds(enemyActionDelay);

        if (bestCard == null)
        {
            Debug.Log("Àû: »ç¿ëÇÒ Ä«µå ¾øÀ½, ÅÏ Á¾·á");
            ChangePhase(BattlePhase.TurnEnd);
            yield break;
        }

        Debug.Log($"ÀûÀÌ [{bestCard.data.cardName}] »ç¿ë");
        uiManager.ShowEnemyAction(bestCard);

        yield return new WaitForSeconds(0.5f);

        CurrentState = gameStateManager.ApplyAction(CurrentState, bestCard);
        UpdateUI(); // OnStateUpdated Áßº¹ È£Ãâ Á¦°Å

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
        // ¼ÕÆÐ ¡æ discardPile·Î ÀÌµ¿
        foreach (CardInstance card in CurrentState.hand)
            CurrentState.discardPile.Add(card);
        CurrentState.hand.Clear();

        // »óÅÂÀÌ»ó Ã³¸®
        ApplyStatusEffects();

        // »óÅÂÀÌ»ó Ã³¸® ÈÄ UI µ¿±âÈ­
        UpdateUI();

        if (CurrentState.IsTerminal())
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

        StopAllCoroutines();

        OnBattleEnd?.Invoke(playerWin);
        uiManager.ShowBattleResult(playerWin);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // UI ¾÷µ¥ÀÌÆ® (OnStateUpdated ÀÌº¥Æ® + UIManager µ¿½Ã Ã³¸®)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void UpdateUI()
    {
        OnStateUpdated?.Invoke(CurrentState);
        uiManager.UpdateAll(CurrentState);
    }
}
