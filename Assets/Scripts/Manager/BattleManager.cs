using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;  //테스트용

/// <summary>
/// 배틀씬의 전체 흐름을 관리하는 class
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    // =====================================================
    // Controller
    // =====================================================

    private IEntityController playerController;
    private IEntityController enemyController;

    [Header("Presentation")]
    [SerializeField] private CardPlayPresentation cardPlayPresentation;

    /// <summary>
    /// 현재 턴의 Controller를 리턴
    /// </summary>
    private IEntityController GetCurrentController()
    {
        if (currentTurnEntity == GameData.instance.player)
        {
            return playerController;
        }

        return enemyController;
    }

    // =====================================================
    // 현재 턴 Entity
    // =====================================================

    [HideInInspector] public Entity currentTurnEntity;

    // =====================================================
    // 현재 턴 상태
    // =====================================================

    public enum BattlePhase
    {
        PlayerTurn,
        EnemyTurn,
    }

    [HideInInspector] public BattlePhase battlePhase;

    // =====================================================
    // Turn Setting
    // =====================================================

    [Header("Turn Setting")]
    [HideInInspector] public int drawPerTurn = 5;

    // =====================================================
    // 현재 턴 수
    // =====================================================

    [HideInInspector] public int turnCount = 1;

    // =====================================================
    // 상태 변수
    // =====================================================

    private bool isBusy;
    public bool IsBusy => isBusy;

    [Header("Enemy Card Preview")]
    [SerializeField] private float enemyCardPreviewDuration = 1.3f;
    [SerializeField] private float enemyCardActionInterval = 0.4f;

    // =====================================================
    // Unity
    // =====================================================

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        GameData.instance.player.OnDead -= OnPlayerDead;
        GameData.instance.enemy.OnDead -= OnEnemyDead;
    }

    // =====================================================
    // 전투 시작
    // =====================================================

    public void Init()
    {
        InitializeBattle();

        // 전투 데이터 초기화는 끝났지만,
        // 첫 턴 시작은 페이드인 완료 후 진행
        StartCoroutine(WaitForSceneReadyAndStartBattle());
    }

    private IEnumerator WaitForSceneReadyAndStartBattle()
    {
        // SceneChanger가 없는 테스트 환경도 지원
        if (SceneChanger.instance != null)
        {
            yield return new WaitUntil(() =>
                SceneChanger.instance != null &&
                SceneChanger.instance.IsReady
            );
        }

        // 페이드인이 끝난 뒤에만 첫 턴 시작
        if (!battleEnded)
        {
            StartPlayerTurn();
        }
    }

    private void InitializeBattle()
    {
        battleEnded = false;
        battleEnding = false;
        relicBattleStarted = false;

        isBusy = true;
        turnCount = 1;

        MCTSTranspositionTable.Clear();

        GameData.instance.player.ClearStatuses();
        GameData.instance.enemy.ClearStatuses();

        InitializeControllers();

        GameData.instance.player.OnDead -= OnPlayerDead;
        GameData.instance.enemy.OnDead -= OnEnemyDead;

        GameData.instance.player.OnDead += OnPlayerDead;
        GameData.instance.enemy.OnDead += OnEnemyDead;

        // 캐릭터 애니메이션 연출 초기화
        if (cardPlayPresentation != null)
        {
            cardPlayPresentation.Initialize(
                GameData.instance.player,
                GameData.instance.enemy
            );
        }
        else
        {
            Debug.LogWarning(
                "BattleManager: CardPlayPresentation이 연결되지 않았습니다.",
                this
            );
        }
    }



    // =====================================================
    // Controller 초기화
    // =====================================================

    private void InitializeControllers()
    {
        playerController =
            CreateController(SimulationManager.instance.PlayerControllerType);

        enemyController =
            CreateController(SimulationManager.instance.EnemyControllerType);
    }

    /// <summary>
    /// ControllerType에 맞는 Controller 생성
    /// </summary>
    private IEntityController CreateController(
        ControllerType controllerType)
    {
        switch (controllerType)
        {
            case ControllerType.MCTS:
                return new MCTSController();

            case ControllerType.AlphaBeta:
                return new AlphaBetaController();
            
            case ControllerType.Greedy:
                return new GreedyController();

            case ControllerType.Minimax:
                return new MinimaxController();

            case ControllerType.Random:
                return new RandomController();

            case ControllerType.Human:
            default:
                return null;
        }
    }

    public void SetEnemyAnimatorController(
    RuntimeAnimatorController controller)
    {
        if (cardPlayPresentation == null)
        {
            Debug.LogError(
                "BattleManager: CardPlayPresentation이 연결되지 않았습니다.",
                this
            );

            return;
        }

        cardPlayPresentation.SetEnemyAnimatorController(
            controller
        );
    }

    public void SetEnemyVisual(EnemyData enemyData)
    {
        if (cardPlayPresentation == null)
        {
            Debug.LogError(
                "BattleManager: CardPlayPresentation이 연결되지 않았습니다.",
                this
            );

            return;
        }

        cardPlayPresentation.SetEnemyVisual(enemyData);
    }

    // =====================================================
    // 플레이어 턴 시작
    // =====================================================

    private void StartPlayerTurn()
    {
        battlePhase = BattlePhase.PlayerTurn;

        currentTurnEntity = GameData.instance.player;

        //UI로 턴 표시
        BattleUIManager.instance.ShowTurnBanner(
        turnCount,
        battlePhase);


        StartCoroutine(PlayerTurnRoutine());
    }

    private IEnumerator PlayerTurnRoutine()
    {
        isBusy = true;

        // 플레이어 턴 시작 시 방어도 초기화
        currentTurnEntity.Block = 0;

        // 기본 에너지 회복
        RecoverEnergy(currentTurnEntity);

        RelicManager relicManager = RelicManager.EnsureInstance();

        // 전투 시작 유물 효과: 전투당 한 번
        if (!relicBattleStarted)
        {
            relicBattleStarted = true;

            relicManager.NotifyBattleStarted(
                GameData.instance.player,
                GameData.instance.enemy
            );
        }

        if (battleEnded)
            yield break;

        // 플레이어 턴 시작 유물 효과
        relicManager.NotifyTurnStarted(
            GameData.instance.player
        );

        if (battleEnded)
            yield break;

        // 카드 드로우
        yield return StartCoroutine(DrawPlayerCardsRoutine());

        if (battleEnded)
            yield break;

        isBusy = false;

        // AI가 플레이하는 경우
        if (GetCurrentController() != null)
        {
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(AIPlayRoutine());

            if (!battleEnded)
                EndTurn();
        }
    }

    // =====================================================
    // 플레이어 턴 종료
    // =====================================================

    /// <summary>
    /// 턴 종료 버튼을 눌렀을 때 호출
    /// </summary>
    public void EndTurn()
    {
        if (isBusy)
            return;

        if (battlePhase == BattlePhase.PlayerTurn)
        {
            StartCoroutine(PlayerEndTurnRoutine());
        }
    }

    private IEnumerator PlayerEndTurnRoutine()
    {
        isBusy = true;

        // 남은 에너지 제거
        currentTurnEntity.CurrentEnergy = 0;

        // 손패 버리기
        yield return StartCoroutine(DiscardPlayerHandRoutine());

        if (battleEnded)
            yield break;

        // 플레이어 턴 종료 유물 효과
        RelicManager.EnsureInstance().NotifyTurnEnded(
            GameData.instance.player
        );

        // 디버프 지속 시간 감소
        currentTurnEntity.TickStatuses();

        yield return new WaitForSeconds(0.2f);

        if (battleEnded)
            yield break;

        StartEnemyTurn();
    }

    // =====================================================
    // 적 턴 시작
    // =====================================================

    private void StartEnemyTurn()
    {
        battlePhase = BattlePhase.EnemyTurn;

        currentTurnEntity = GameData.instance.enemy;

        //UI로 턴 표시
        BattleUIManager.instance.ShowTurnBanner(
        turnCount,
        battlePhase);
        
        
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        isBusy = true;

        // 턴 시작 시 Block 제거
        currentTurnEntity.Block = 0;

        // 에너지 회복
        RecoverEnergy(currentTurnEntity);

        // 카드 드로우
        EnemyDeckManager.instance.DrawCards(drawPerTurn);

        yield return new WaitForSeconds(0.5f);

        // AI 행동
        yield return StartCoroutine(AIPlayRoutine());

        yield return new WaitForSeconds(0.5f);

        // 손패 버리기
        DiscardEnemyHand();

        // 턴 종료 시 디버프들의 지속 턴 수를 1만큼 감소
        currentTurnEntity.TickStatuses();

        // 턴 증가
        turnCount++;

        StartPlayerTurn();
    }

    // =====================================================
    // AI 행동 루틴
    // =====================================================

    private IEnumerator AIPlayRoutine()
    {
        Entity entity = currentTurnEntity;

        IEntityController controller = GetCurrentController();

        if (controller == null)
            yield break;

        // 적 턴인지 여부
        bool showPreview = (entity == GameData.instance.enemy);

        while (true)
        {
            // 1. 카드 선택
            CardInstance selectedCard = controller.SelectCard(entity);

            if (selectedCard == null || selectedCard.data == null)
                break;

            // 2. 카드 사용 가능 여부를 확인하고 연출 실행
            if (showPreview)
            {
                // 카드 프리뷰 표시
                EnemyCardPreviewUIManager.instance.Show(selectedCard);
            }

            // 3. 카드 사용
            bool success = TryUseCard(entity, selectedCard);

            if (!success)
                break;

            // 4. 적 애니메이션 연출 대기
            if (showPreview)
            {
                yield return new WaitForSeconds(enemyCardPreviewDuration);

                EnemyCardPreviewUIManager.instance.Hide();
            }

            // 5. 다음 행동 간격
            yield return new WaitForSeconds(enemyCardActionInterval);
        }

        // 마지막 정리
        if (showPreview)
        {
            EnemyCardPreviewUIManager.instance.Hide();
        }
    }

    // =====================================================
    // 플레이어 카드 드로우
    // =====================================================

    private IEnumerator DrawPlayerCardsRoutine()
    {
        for (int i = 0; i < drawPerTurn; i++)
        {
            PlayerDeckManager.instance.DrawCard();

            yield return new WaitForSeconds(0.15f);
        }
    }

    // =====================================================
    // 플레이어 손패 버리기
    // =====================================================

    private IEnumerator DiscardPlayerHandRoutine()
    {
        HandManager hand = HandManager.instance;

        while (hand.cards.Count > 0)
        {
            CardView cardView =
                hand.cards[hand.cards.Count - 1];

            PlayerDeckManager.instance
                .DiscardCard(cardView.card);

            yield return new WaitForSeconds(0.08f);
        }
    }

    // =====================================================
    // 적 손패 버리기
    // =====================================================

    private void DiscardEnemyHand()
    {
        while (GameData.instance.enemy.hand.Count > 0)
        {
            CardInstance card =
                GameData.instance.enemy.hand[
                    GameData.instance.enemy.hand.Count - 1];

            EnemyDeckManager.instance.DiscardCard(card);
        }
    }

    // =====================================================
    // 에너지 회복
    // =====================================================

    private void RecoverEnergy(Entity entity)
    {
        entity.CurrentEnergy = entity.MaxEnergy;
    }

    // =====================================================
    // 전투 결과
    // =====================================================

    [Header("Death Animation")]
    [SerializeField] private float deathAnimationWait = 1.0f;

    private bool battleEnded;
    private bool battleEnding;
    private bool relicBattleStarted;

    // =====================================================
    // 전투 종료 및 사망 처리
    // =====================================================

    private void OnPlayerDead()
    {
        BeginBattleEnd(false);
    }

    private void OnEnemyDead()
    {
        BeginBattleEnd(true);
    }

    private void BeginBattleEnd(bool playerWin)
    {
        if (battleEnded || battleEnding)
            return;

        battleEnding = true;
        isBusy = true;

        // 진행 중인 턴 및 AI 코루틴 중단
        StopAllCoroutines();

        // 사망 애니메이션 재생
        if (cardPlayPresentation != null)
        {
            Entity deadEntity = playerWin
                ? GameData.instance.enemy
                : GameData.instance.player;

            cardPlayPresentation.PlayDeathAnimation(deadEntity);
        }

        // 사망 애니메이션이 재생된 후 결과 처리
        StartCoroutine(
            FinishBattleAfterDeathAnimation(playerWin)
        );
    }

    private IEnumerator FinishBattleAfterDeathAnimation(
        bool playerWin)
    {
        yield return new WaitForSeconds(deathAnimationWait);

        FinishBattle(playerWin);
    }

    private void FinishBattle(bool playerWin)
    {
        if (battleEnded)
            return;

        battleEnded = true;
        battleEnding = false;
        isBusy = true;

        // 전투 종료 유물 효과
        RelicManager.EnsureInstance().NotifyBattleEnded(playerWin);

        bool autoSimulation =
            SimulationManager.instance != null &&
            SimulationManager.instance.AutoSimulation;

        if (playerWin)
            Debug.Log("플레이어 승리!");
        else
            Debug.Log("플레이어 패배...");

        // 자동 시뮬레이션
        if (autoSimulation)
        {
            SimulationManager.instance.OnBattleEnded(playerWin);
            return;
        }

        // 일반 게임에서 패배
        if (!playerWin)
        {
            // 추후 게임 오버 UI 및 런 종료 처리를 추가할 위치
            return;
        }

        // 일반 게임에서 승리
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.ShowRewards();
        }
        else
        {
            Debug.LogError(
                "RewardManager.Instance가 없습니다."
            );
        }
    }









    // =====================================================
    // 카드 사용
    // =====================================================

    /// <summary>
    /// 카드 사용 시도
    /// </summary>
    public bool TryUseCard(
        Entity user,
        CardInstance card)
    {
        // =====================================================
        // 카드 사용 가능 여부 확인
        // =====================================================

        // 현재 턴 Entity만 사용 가능
        if (user != currentTurnEntity)
        {
            return false;
        }

        // 실제 손패에 존재해야 함
        if (!user.hand.Contains(card))
        {
            return false;
        }

        // 에너지 부족
        if (!user.TryUseEnergy(card.currentCost))
        {
            return false;
        }

        // =====================================================
        // 카드 효과 실행
        // =====================================================

        if (cardPlayPresentation != null)
        {
            cardPlayPresentation.PlayCardAnimation(
                user,
                card.data.cardType
            );
        }

        ICardEffect effect = CardEffectFactory.Create(card);

        Entity target = GetCardTarget(card);

        effect?.Execute(user, target);

        if (card.data.cardType == CardType.Attack && user.DoubleTapCharges > 0)
        {
            effect?.Execute(user, target);
            user.DoubleTapCharges -= 1;
        }
        // =====================================================
        // 카드 처리
        // =====================================================

        if (user == GameData.instance.player)
        {
            // 파워 카드
            if (card.data.cardType == CardType.Power)
            {
                user.hand.Remove(card);

                // UI 제거
                CardView view =
                    HandManager.instance.cards.Find(c => c.card == card);

                if (view != null)
                {
                    HandManager.instance.RemoveCard(view);
                }
            }

            // 소멸 카드
            else if (card.exhaust)
            {
                PlayerDeckManager.instance.ExhaustCard(card);
            }

            // 일반적인 카드
            else
            {
                PlayerDeckManager.instance.DiscardCard(card);
            }
        }

        else if (user == GameData.instance.enemy)
        {
            // 파워 카드
            if (card.data.cardType == CardType.Power)
            {
                user.hand.Remove(card);
            }

            // 소멸 카드
            else if (card.exhaust)
            {
                EnemyDeckManager.instance.ExhaustCard(card);
            }

            // 일반 카드
            else
            {
                EnemyDeckManager.instance.DiscardCard(card);
            }
        }

        return true;
    }


    // =====================================================
    // CardEffect에서 사용
    // =====================================================

    /// <summary>
    /// 카드의 타겟 Entity를 리턴
    /// </summary>
    private Entity GetCardTarget(CardInstance card)
    {
        switch (card.data.targetType)
        {
            case TargetType.Self:
                return currentTurnEntity;

            case TargetType.Enemy:
                return GetOpponent(currentTurnEntity);

            default:
                Debug.LogError(
                    $"Unknown TargetType : {card.data.targetType}");

                return null;
        }
    }

    private Entity GetOpponent(Entity entity)
    {
        if (entity == GameData.instance.player)
        {
            return GameData.instance.enemy;
        }

        return GameData.instance.player;
    }
}
