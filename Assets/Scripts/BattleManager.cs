using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePhase
{
    Idle,           // 대기
    BattleStart,    // 전투 시작
    PlayerTurn,     // 플레이어 턴
    EnemyTurn,      // 적 턴
    TurnEnd,        // 턴 종료 처리
    BattleEnd       // 전투 종료
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

    [Header("Database")]
    [SerializeField] private CardDatabase cardDatabase;

    // ─── 상태 ──────────────────────────────────
    public GameState CurrentState { get; private set; }
    public BattlePhase CurrentPhase { get; private set; }

    // ─── 이벤트 ────────────────────────────────
    public event Action<BattlePhase> OnPhaseChanged;
    public event Action<GameState> OnStateUpdated;
    public event Action<bool> OnBattleEnd;   // true = 플레이어 승리

    // ─── 설정 ──────────────────────────────────
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

    // ─────────────────────────────────────────────
    // 전투 시작
    // ─────────────────────────────────────────────
    public void StartBattle(Entity player, Entity enemy, List<CardDatabase.CardEntry> playerDeck)
    {
        CurrentState = new GameState
        {
            player = player,
            enemy = enemy,
            deck = playerDeck,
            hand = new List<CardDatabase.CardEntry>(),
            discardPile = new List<CardDatabase.CardEntry>(),
            
            //GameState에서 가져와야하지않을까?
            currentEnergy = 3,
            maxEnergy = 3,
            turnCount = 0,
            isPlayerTurn = true
        };

        gameStateManager.ShuffleDeck(CurrentState.deck);
        ChangePhase(BattlePhase.BattleStart);
    }

    // ─────────────────────────────────────────────
    // 페이즈 전환
    // ─────────────────────────────────────────────
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

    // ─────────────────────────────────────────────
    // 전투 시작 처리
    // ─────────────────────────────────────────────
    private void HandleBattleStart()
    {
        Debug.Log("전투 시작!");

        // 첫 드로우는 BattleStart에서만
        CurrentState = gameStateManager.DrawCards(CurrentState, drawCount);
        UpdateUI();

        if (battleMode == BattleMode.Normal)
            enemyController.PrepareNextAction(CurrentState);

        ChangePhase(BattlePhase.PlayerTurn);
    }

    // ─────────────────────────────────────────────
    // 플레이어 턴 시작
    // ─────────────────────────────────────────────
    private void HandlePlayerTurnStart()
    {
        Debug.Log($"[턴 {CurrentState.turnCount}] 플레이어 턴 시작");

        // 에너지 충전
        CurrentState.currentEnergy = CurrentState.maxEnergy;

        // 방어도 초기화
        CurrentState.player.block = 0;

        if (battleMode == BattleMode.Normal)
            uiManager.ShowEnemyIntent(enemyController.NextIntent);

        UpdateUI();
        // 이후 플레이어 입력 대기
    }

    // ─────────────────────────────────────────────
    // 플레이어 카드 사용 (UI에서 호출)
    // ─────────────────────────────────────────────
    public void PlayerUseCard(CardDatabase.CardEntry card)
    {
        if (CurrentPhase != BattlePhase.PlayerTurn)
        {
            Debug.LogWarning("플레이어 턴이 아닙니다.");
            return;
        }

        if (card.cost > CurrentState.currentEnergy)
        {
            Debug.LogWarning("에너지가 부족합니다.");
            uiManager.ShowEnergyWarning();
            return;
        }

        Debug.Log($"플레이어가 [{card.data.cardName}] 사용");

        CurrentState = gameStateManager.ApplyAction(CurrentState, card);
        UpdateUI();

        if (CurrentState.IsTerminal())
            ChangePhase(BattlePhase.BattleEnd);
    }

    // ─────────────────────────────────────────────
    // 플레이어 턴 종료 (버튼에서 호출)
    // ─────────────────────────────────────────────
    public void PlayerEndTurn()
    {
        if (CurrentPhase != BattlePhase.PlayerTurn) return;

        Debug.Log("플레이어 턴 종료");
        ChangePhase(BattlePhase.EnemyTurn);
    }

    // ─────────────────────────────────────────────
    // 적 턴 처리
    // ─────────────────────────────────────────────
    private void HandleEnemyTurn()
    {
        Debug.Log("적 턴 시작");

        // 적 방어도 초기화
        CurrentState.enemy.block = 0;

        StartCoroutine(battleMode == BattleMode.Normal
            ? NormalEnemyTurnCoroutine()
            : HardEnemyTurnCoroutine());
    }

    // 노말 모드: 패턴 AI
    private IEnumerator NormalEnemyTurnCoroutine()
    {
        uiManager.ShowEnemyAction(enemyController.NextIntent);
        yield return new WaitForSeconds(enemyActionDelay);

        CurrentState = enemyController.ExecuteAction(CurrentState);
        UpdateUI(); // OnStateUpdated 중복 호출 제거 (UpdateUI 내부에서 처리)

        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            yield break;
        }

        ChangePhase(BattlePhase.TurnEnd);
    }

    // 하드 모드: MCTS AI
    private IEnumerator HardEnemyTurnCoroutine()
    {
        uiManager.ShowEnemyThinking();  // "생각 중..." 같은 UI

        // MCTS 실행 (비동기)
        CardDatabase.CardEntry bestCard = null;
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
            Debug.Log("적: 사용할 카드 없음, 턴 종료");
            ChangePhase(BattlePhase.TurnEnd);
            yield break;
        }

        Debug.Log($"적이 [{bestCard.data.cardName}] 사용");
        uiManager.ShowEnemyAction(bestCard);

        yield return new WaitForSeconds(0.5f);

        CurrentState = gameStateManager.ApplyAction(CurrentState, bestCard);
        UpdateUI(); // OnStateUpdated 중복 호출 제거

        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            yield break;
        }

        ChangePhase(BattlePhase.TurnEnd);
    }

    // ─────────────────────────────────────────────
    // 턴 종료 처리
    // ─────────────────────────────────────────────
    private void HandleTurnEnd()
    {
        // 손패 → discardPile로 이동
        foreach (CardInstance card in CurrentState.hand)
            CurrentState.discardPile.Add(card);
        CurrentState.hand.Clear();

        // 상태이상 처리
        ApplyStatusEffects();

        // 상태이상 처리 후 UI 동기화
        UpdateUI();

        if (CurrentState.IsTerminal())
        {
            ChangePhase(BattlePhase.BattleEnd);
            return;
        }

        // 드로우
        CurrentState = gameStateManager.DrawCards(CurrentState, drawCount);
        CurrentState.turnCount++;

        UpdateUI();
        ChangePhase(BattlePhase.PlayerTurn);
    }

    // ─────────────────────────────────────────────
    // 상태이상 처리
    // ─────────────────────────────────────────────
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
            // 턴마다 감소시키려면 아래 주석 해제
            // entity.statusEffects[EffectType.Burn]--;
            // if (entity.statusEffects[EffectType.Burn] <= 0)
            //     entity.statusEffects.Remove(EffectType.Burn);
        }
    }

    // ─────────────────────────────────────────────
    // 전투 종료
    // ─────────────────────────────────────────────
    private void HandleBattleEnd()
    {
        bool playerWin = CurrentState.enemy.currentHP <= 0;
        Debug.Log(playerWin ? "플레이어 승리!" : "플레이어 패배...");

        StopAllCoroutines();

        OnBattleEnd?.Invoke(playerWin);
        uiManager.ShowBattleResult(playerWin);
    }

    // ─────────────────────────────────────────────
    // UI 업데이트 (OnStateUpdated 이벤트 + UIManager 동시 처리)
    // ─────────────────────────────────────────────
    private void UpdateUI()
    {
        OnStateUpdated?.Invoke(CurrentState);
        uiManager.UpdateAll(CurrentState);
    }
}
