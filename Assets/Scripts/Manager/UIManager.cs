using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 탑 패널의 UI들(체력, 골드, 포션슬롯, 지도, 현재 덱, 설정)
///     ,드로우 파일, 버린 카드 파일, 소멸 카드 파일, 에너지, 턴 종료 버튼들을 관리하는 class
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("HP")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Transform hpTransform;

    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Transform goldTransform;

    [Header("Deck Count")]
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Transform deckCountTransform;

    [Header("Draw Pile")]
    [SerializeField] private TMP_Text drawPileText;
    [SerializeField] private Transform drawPileTransform;

    [Header("Discard Pile")]
    [SerializeField] private TMP_Text discardPileText;
    [SerializeField] private Transform discardPileTransform;

    [Header("Exhaust Pile")]
    [SerializeField] private TMP_Text exhaustPileText;
    [SerializeField] private Transform exhaustPileTransform;

    [Header("Energy")]
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Transform energyTransform;

    [Header("Player HP Gauge")]
    [SerializeField] private TMP_Text hpBarText;
    [SerializeField] private Transform hpBarTextTransform;
    [SerializeField] private PlayerHPBarUI playerHPBarUI;

    [Header("Player Block")]
    [SerializeField] private GameObject playerBlockUI;
    [SerializeField] private TMP_Text playerBlockText;
    [SerializeField] private Transform playerBlockTransform;

    [Header("Enemy HP Gauge")]
    [SerializeField] private TMP_Text enemyHPBarText;
    [SerializeField] private Transform enemyHPBarTextTransform;
    [SerializeField] private EnemyHPBarUI enemyHPBarUI;
    
    [Header("Enemy Block")]
    [SerializeField] private GameObject enemyBlockUI;
    [SerializeField] private TMP_Text enemyBlockText;
    [SerializeField] private Transform enemyBlockTransform;



    [Header("Warning UI")]
    [SerializeField] private GameObject warningBubble;
    [SerializeField] private RectTransform warningBubbleRect;
    [SerializeField] private TMP_Text warningText;

    private Coroutine warningRoutine;
    private Tween warningTween;

    [Header("Turn Banner")]
    [SerializeField] private TurnBanner turnBanner;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // =====================================================
        // 이벤트 등록
        // =====================================================

        // 플레이어의 HP
        GameData.instance.player.OnCurrentHPChanged += UpdatePlayerHP;
        GameData.instance.player.OnMaxHPChanged += UpdatePlayerHP;

        // 적의 HP
        GameData.instance.enemy.OnCurrentHPChanged += UpdateEnemyHP;
        GameData.instance.enemy.OnMaxHPChanged += UpdateEnemyHP;

        // 플레이어의 방어도
        GameData.instance.player.OnBlockChanged += UpdatePlayerBlock;

        // 적의 방어도
        GameData.instance.enemy.OnBlockChanged += UpdateEnemyBlock;

        // 플레이어 골드
        GameData.instance.player.OnGoldChanged += UpdateGold;

        // 플레이어 덱의 카드 수
        GameData.instance.player.OnDeckCountChanged += UpdatePlayerDeckCount;

        // 플레이어 뽑을 카드 더미의 카드 수
        GameData.instance.player.OnDrawPileCountChanged += UpdatePlayerDrawPileCount;

        // 플레이어 버린 카드 더미의 카드 수
        GameData.instance.player.OnDiscardPileCountChanged += UpdatePlayerDiscardPileCount;

        // 플레이어 소멸된 카드 더미의 카드 수
        GameData.instance.player.OnExhaustPileCountChanged += UpdatePlayerExhaustPileCount;

        // 플레이어의 에너지
        GameData.instance.player.OnCurrentEnergyChanged += UpdatePlayerEnergy;
        GameData.instance.player.OnMaxEnergyChanged += UpdatePlayerEnergy;

        

        // Initial UI Update
        UpdateAll();
    }

    private void OnDisable()
    {
        // =====================================================
        // 이벤트 해제
        // =====================================================

        // 플레이어의 HP
        GameData.instance.player.OnCurrentHPChanged -= UpdatePlayerHP;
        GameData.instance.player.OnMaxHPChanged -= UpdatePlayerHP;

        // 적의 HP
        GameData.instance.enemy.OnCurrentHPChanged -= UpdateEnemyHP;
        GameData.instance.enemy.OnMaxHPChanged -= UpdateEnemyHP;

        // 플레이어의 방어도
        GameData.instance.player.OnBlockChanged -= UpdatePlayerBlock;

        // 적의 방어도
        GameData.instance.enemy.OnBlockChanged -= UpdateEnemyBlock;

        // 플레이어 골드
        GameData.instance.player.OnGoldChanged -= UpdateGold;

        // 플레이어 덱의 카드 수
        GameData.instance.player.OnDeckCountChanged -= UpdatePlayerDeckCount;

        // 플레이어 뽑을 카드 더미의 카드 수
        GameData.instance.player.OnDrawPileCountChanged -= UpdatePlayerDrawPileCount;

        // 플레이어 버린 카드 더미의 카드 수
        GameData.instance.player.OnDiscardPileCountChanged -= UpdatePlayerDiscardPileCount;

        // 플레이어 소멸된 카드 더미의 카드 수
        GameData.instance.player.OnExhaustPileCountChanged -= UpdatePlayerExhaustPileCount;

        // 플레이어의 에너지
        GameData.instance.player.OnCurrentEnergyChanged -= UpdatePlayerEnergy;
        GameData.instance.player.OnMaxEnergyChanged -= UpdatePlayerEnergy;
    }


    // =====================================================
    // 모든 UI 갱신
    // =====================================================

    public void UpdateAll()
    {
        // HP
        UpdatePlayerHP(0);
        UpdateEnemyHP(0);

        // Block
        UpdatePlayerBlock(GameData.instance.player.Block);
        UpdateEnemyBlock(GameData.instance.enemy.Block);

        // Gold
        UpdateGold(GameData.instance.player.Gold);

        // Deck Count
        UpdatePlayerDeckCount(GameData.instance.player.DeckCount);

        // Draw Pile
        UpdatePlayerDrawPileCount(GameData.instance.player.DrawPileCount);

        // Discard Pile
        UpdatePlayerDiscardPileCount(GameData.instance.player.DiscardPileCount);

        // Exhaust Pile
        UpdatePlayerExhaustPileCount(GameData.instance.player.ExhaustPileCount);

        // Energy
        UpdatePlayerEnergy(0);
    }


    // =====================================================
    // 플레이어의 HP
    // =====================================================

    private void UpdatePlayerHP(int _)
    {
        hpText.text =
            GameData.instance.player.CurrentHP +
            "/" +
            GameData.instance.player.MaxHP;

        hpBarText.text =
            GameData.instance.player.CurrentHP +
            "/" +
            GameData.instance.player.MaxHP;

        PlayPunchAnimation(hpTransform);
        PlayPunchAnimation(hpBarTextTransform);

        playerHPBarUI.RefreshPlayerHPBar();
    }

    // =====================================================
    // 적의 HP
    // =====================================================
    private void UpdateEnemyHP(int _)
    {
        enemyHPBarText.text =
            GameData.instance.enemy.CurrentHP +
            "/" +
            GameData.instance.enemy.MaxHP;

        PlayPunchAnimation(enemyHPBarTextTransform);

        enemyHPBarUI.RefreshEnemyHPBar();
    }

    // =====================================================
    // 플레이어의 방어도
    // =====================================================

    private void UpdatePlayerBlock(int value)
    {
        bool active = value > 0;

        playerBlockUI.SetActive(active);

        if (!active)
            return;

        playerBlockText.text = value.ToString();

        PlayPunchAnimation(playerBlockTransform);
    }

    // =====================================================
    // 적의 방어도
    // =====================================================

    private void UpdateEnemyBlock(int value)
    {
        bool active = value > 0;

        enemyBlockUI.SetActive(active);

        if (!active)
            return;

        enemyBlockText.text = value.ToString();

        PlayPunchAnimation(enemyBlockTransform);
    }

    // =====================================================
    // Gold
    // =====================================================

    private void UpdateGold(int value)
    {
        goldText.text = value.ToString();

        PlayPunchAnimation(goldTransform);
    }

    // =====================================================
    // Deck Count
    // =====================================================

    private void UpdatePlayerDeckCount(int value)
    {
        deckCountText.text = value.ToString();

        PlayPunchAnimation(deckCountTransform);
    }

    // =====================================================
    // Draw Pile
    // =====================================================

    private void UpdatePlayerDrawPileCount(int value)
    {
        drawPileText.text = value.ToString();

        PlayPunchAnimation(drawPileTransform);
    }

    // =====================================================
    // Discard Pile
    // =====================================================

    private void UpdatePlayerDiscardPileCount(int value)
    {
        discardPileText.text = value.ToString();

        PlayPunchAnimation(discardPileTransform);
    }

    // =====================================================
    // Exhaust Pile
    // =====================================================

    private void UpdatePlayerExhaustPileCount(int value)
    {
        exhaustPileText.text = value.ToString();

        PlayPunchAnimation(exhaustPileTransform);
    }

    // =====================================================
    // Energy
    // =====================================================

    private void UpdatePlayerEnergy(int _)
    {
        energyText.text =
            GameData.instance.player.CurrentEnergy +
            "/" +
            GameData.instance.player.MaxEnergy;

        PlayPunchAnimation(energyTransform);
    }

    // =====================================================
    // Animation
    // =====================================================

    public void PlayPunchAnimation(Transform target)
    {
        StartCoroutine(PunchRoutine(target));
    }

    private IEnumerator PunchRoutine(Transform target)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 enlargedScale = Vector3.one * 1.1f;

        float duration = 0.08f;

        float t = 0f;

        // 커짐
        while (t < duration)
        {
            t += Time.deltaTime;

            target.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    t / duration);

            yield return null;
        }

        t = 0f;

        // 원래 크기로
        while (t < duration)
        {
            t += Time.deltaTime;

            target.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    t / duration);

            yield return null;
        }

        target.localScale = originalScale;
    }



    // =====================================================
    // 경고 말풍선 출력
    // =====================================================

    //경고 메시지 종류
    public enum WarningType
    {
        NotEnoughEnergy,
        HandFull,
        NoCardsToDraw
    }

    public void ShowWarning(WarningType type)
    {
        string message = type switch
        {
            WarningType.NotEnoughEnergy => "I don't have enough energy..",
            WarningType.HandFull => "My hand is full!",
            WarningType.NoCardsToDraw => "There are no cards to draw!",
            _ => ""
        };

        if (warningRoutine != null)
            StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningBubble.SetActive(true);

        // 이전 애니메이션 정리
        warningTween?.Kill();

        SetWarningMessage(message);

        RectTransform rect = warningBubbleRect;

        // 초기 상태
        rect.localScale = Vector3.one * 0.8f;

        // 등장 애니메이션
        warningTween = rect.DOScale(1.15f, 0.15f).SetEase(Ease.OutBack);

        warningTween = rect.DOShakeAnchorPos(
            2.5f,
            7f,
            10,
            90f
        );

        yield return new WaitForSeconds(1.4f);

        // 종료 애니메이션
        warningTween = rect.DOScale(0.8f, 0.15f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.15f);

        warningBubble.SetActive(false);
    }

    /// <summary>
    /// 경고 메시지의 길이에 따라 말풍선의 크기 자동으로 조절, 말풍선에 메시지 입력
    /// </summary>
    /// <param name="message"></param>
    private void SetWarningMessage(string message)
    {
        warningText.text = message;

        Canvas.ForceUpdateCanvases();
        warningText.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(warningText.rectTransform);

        warningBubbleRect.sizeDelta = new Vector2(warningText.preferredWidth - 80f, warningText.preferredHeight - 20f);
    }

    // =====================================================
    // 현재 턴 표시
    // =====================================================
    public void ShowTurnBanner(
    int turnCount,
    BattleManager.BattlePhase phase)
    {
        turnBanner.ShowTurn(turnCount, phase);
    }
}
