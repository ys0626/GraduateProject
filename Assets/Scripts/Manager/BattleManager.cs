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
    // 테스트용 버튼
    // =====================================================

    [SerializeField] private GameObject restartButton;

    // =====================================================
    // Controller
    // =====================================================

    private IEntityController playerController;
    private IEntityController enemyController;

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
    public int drawPerTurn = 5;

    // =====================================================
    // 현재 턴 수
    // =====================================================

    [HideInInspector] public int turnCount = 1;

    // =====================================================
    // 상태 변수
    // =====================================================

    private bool isBusy;
    public bool IsBusy => isBusy;

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
        turnCount = 1;

        GameData.instance.player.statuses.Clear();
        GameData.instance.enemy.statuses.Clear();
        
        InitializeControllers();

        GameData.instance.player.OnDead += OnPlayerDead;
        GameData.instance.enemy.OnDead += OnEnemyDead;

        StartPlayerTurn();
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

            case ControllerType.Random:
                return new RandomController();

            case ControllerType.Greedy:
                return new GreedyController();

            case ControllerType.Human:
            default:
                return null;
        }
    }

    // =====================================================
    // 플레이어 턴 시작
    // =====================================================

    private void StartPlayerTurn()
    {
        battlePhase = BattlePhase.PlayerTurn;

        currentTurnEntity = GameData.instance.player;

        //UI로 턴 표시
        UIManager.instance.ShowTurnBanner(
        turnCount,
        battlePhase);


        StartCoroutine(PlayerTurnRoutine());
    }

    private IEnumerator PlayerTurnRoutine()
    {
        isBusy = true;

        // 턴 시작 시 Block 제거
        currentTurnEntity.Block = 0;

        // 에너지 회복
        RecoverEnergy(currentTurnEntity);

        // 카드 드로우
        yield return StartCoroutine(DrawPlayerCardsRoutine());

        isBusy = false;

        // =====================================================
        // AI가 플레이하는 경우
        // =====================================================

        if (GetCurrentController() != null)
        {
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(AIPlayRoutine());

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

        // 턴 종료 시 디버프들의 지속 턴 수를 1만큼 감소
        currentTurnEntity.TickStatuses();

        yield return new WaitForSeconds(0.2f);

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
        UIManager.instance.ShowTurnBanner(
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

            // 2. Preview 표시 (적 턴일 때만)
            if (showPreview)
            {
                EnemyCardPreviewUIManager.instance.Show(selectedCard);

                // 연출 대기
                yield return new WaitForSeconds(0.8f);
            }

            // 3. 카드 사용
            bool success = TryUseCard(entity, selectedCard);

            if (!success)
                break;

            // 4. UI 정리
            if (showPreview)
            {
                EnemyCardPreviewUIManager.instance.Hide();
            }

            // 5. 다음 행동 간격
            yield return new WaitForSeconds(0.4f);
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

    private void EndBattle(bool playerWin)
    {
        isBusy = true;

        StopAllCoroutines();

        // 자동 시뮬레이션 아닐 때만 버튼 표시
        if (!SimulationManager.instance.AutoSimulation)
        {
            restartButton.SetActive(true);
        }

        if (playerWin)
        {
            Debug.Log("플레이어 승리!");
        }

        else
        {
            Debug.Log("플레이어 패배...");
        }

        // 시뮬레이션 결과 기록
        SimulationManager.instance.OnBattleEnded(playerWin);
    }

    private void OnPlayerDead()
    {
        EndBattle(false);
    }

    private void OnEnemyDead()
    {
        EndBattle(true);
    }

    /// <summary>
    /// 테스트용
    /// 전투 다시 시작
    /// </summary>
    public void RestartBattle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        ICardEffect effect = CardEffectFactory.Create(card);

        Entity target = GetCardTarget(card);

        effect?.Execute(user, target);

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