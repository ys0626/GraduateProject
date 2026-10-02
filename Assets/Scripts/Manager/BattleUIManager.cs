using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager instance;


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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        RegisterEvents();
        UpdateAll();
    }


    private void OnDestroy()
    {
        UnregisterEvents();

        warningTween?.Kill();
    }


    // =====================================================
    // 이벤트 등록
    // =====================================================

    private void RegisterEvents()
    {
        if (GameData.instance == null)
            return;


        if (GameData.instance.player == null)
            return;


        if (GameData.instance.enemy == null)
            return;


        // 플레이어 HP
        GameData.instance.player.OnCurrentHPChanged += UpdatePlayerHP;
        GameData.instance.player.OnMaxHPChanged += UpdatePlayerHP;


        // 적 HP
        GameData.instance.enemy.OnCurrentHPChanged += UpdateEnemyHP;
        GameData.instance.enemy.OnMaxHPChanged += UpdateEnemyHP;


        // 플레이어 방어도
        GameData.instance.player.OnBlockChanged += UpdatePlayerBlock;


        // 적 방어도
        GameData.instance.enemy.OnBlockChanged += UpdateEnemyBlock;


        // Draw Pile
        GameData.instance.player.OnDrawPileCountChanged +=
            UpdatePlayerDrawPileCount;


        // Discard Pile
        GameData.instance.player.OnDiscardPileCountChanged +=
            UpdatePlayerDiscardPileCount;


        // Exhaust Pile
        GameData.instance.player.OnExhaustPileCountChanged +=
            UpdatePlayerExhaustPileCount;


        // Energy
        GameData.instance.player.OnCurrentEnergyChanged +=
            UpdatePlayerEnergy;

        GameData.instance.player.OnMaxEnergyChanged +=
            UpdatePlayerEnergy;
    }


    // =====================================================
    // 이벤트 해제
    // =====================================================

    private void UnregisterEvents()
    {
        if (GameData.instance == null)
            return;


        if (GameData.instance.player == null)
            return;


        if (GameData.instance.enemy == null)
            return;


        // 플레이어 HP
        GameData.instance.player.OnCurrentHPChanged -= UpdatePlayerHP;
        GameData.instance.player.OnMaxHPChanged -= UpdatePlayerHP;


        // 적 HP
        GameData.instance.enemy.OnCurrentHPChanged -= UpdateEnemyHP;
        GameData.instance.enemy.OnMaxHPChanged -= UpdateEnemyHP;


        // 플레이어 방어도
        GameData.instance.player.OnBlockChanged -= UpdatePlayerBlock;


        // 적 방어도
        GameData.instance.enemy.OnBlockChanged -= UpdateEnemyBlock;


        // Draw Pile
        GameData.instance.player.OnDrawPileCountChanged -=
            UpdatePlayerDrawPileCount;


        // Discard Pile
        GameData.instance.player.OnDiscardPileCountChanged -=
            UpdatePlayerDiscardPileCount;


        // Exhaust Pile
        GameData.instance.player.OnExhaustPileCountChanged -=
            UpdatePlayerExhaustPileCount;


        // Energy
        GameData.instance.player.OnCurrentEnergyChanged -=
            UpdatePlayerEnergy;

        GameData.instance.player.OnMaxEnergyChanged -=
            UpdatePlayerEnergy;
    }


    // =====================================================
    // 모든 UI 갱신
    // =====================================================

    public void UpdateAll()
    {
        if (GameData.instance == null)
            return;


        if (GameData.instance.player == null)
            return;


        if (GameData.instance.enemy == null)
            return;


        // Player HP
        UpdatePlayerHP(0);


        // Enemy HP
        UpdateEnemyHP(0);


        // Player Block
        UpdatePlayerBlock(
            GameData.instance.player.Block
        );


        // Enemy Block
        UpdateEnemyBlock(
            GameData.instance.enemy.Block
        );


        // Draw Pile
        UpdatePlayerDrawPileCount(
            GameData.instance.player.DrawPileCount
        );


        // Discard Pile
        UpdatePlayerDiscardPileCount(
            GameData.instance.player.DiscardPileCount
        );


        // Exhaust Pile
        UpdatePlayerExhaustPileCount(
            GameData.instance.player.ExhaustPileCount
        );


        // Energy
        UpdatePlayerEnergy(0);
    }


    // =====================================================
    // 플레이어 HP
    // =====================================================

    private void UpdatePlayerHP(int _)
    {
        if (hpBarText == null)
            return;


        hpBarText.text =
            GameData.instance.player.CurrentHP +
            "/" +
            GameData.instance.player.MaxHP;


        PlayPunchAnimation(hpBarTextTransform);


        if (playerHPBarUI != null)
            playerHPBarUI.RefreshPlayerHPBar();
    }


    // =====================================================
    // 적 HP
    // =====================================================

    private void UpdateEnemyHP(int _)
    {
        if (enemyHPBarText == null)
            return;


        enemyHPBarText.text =
            GameData.instance.enemy.CurrentHP +
            "/" +
            GameData.instance.enemy.MaxHP;


        PlayPunchAnimation(enemyHPBarTextTransform);


        if (enemyHPBarUI != null)
            enemyHPBarUI.RefreshEnemyHPBar();
    }


    // =====================================================
    // 플레이어 방어도
    // =====================================================

    private void UpdatePlayerBlock(int value)
    {
        if (playerBlockUI == null)
            return;


        bool active = value > 0;

        playerBlockUI.SetActive(active);


        if (!active)
            return;


        if (playerBlockText != null)
            playerBlockText.text = value.ToString();


        PlayPunchAnimation(playerBlockTransform);
    }


    // =====================================================
    // 적 방어도
    // =====================================================

    private void UpdateEnemyBlock(int value)
    {
        if (enemyBlockUI == null)
            return;


        bool active = value > 0;

        enemyBlockUI.SetActive(active);


        if (!active)
            return;


        if (enemyBlockText != null)
            enemyBlockText.text = value.ToString();


        PlayPunchAnimation(enemyBlockTransform);
    }


    // =====================================================
    // Draw Pile
    // =====================================================

    private void UpdatePlayerDrawPileCount(int value)
    {
        if (drawPileText == null)
            return;


        drawPileText.text = value.ToString();

        PlayPunchAnimation(drawPileTransform);
    }


    // =====================================================
    // Discard Pile
    // =====================================================

    private void UpdatePlayerDiscardPileCount(int value)
    {
        if (discardPileText == null)
            return;


        discardPileText.text = value.ToString();

        PlayPunchAnimation(discardPileTransform);
    }


    // =====================================================
    // Exhaust Pile
    // =====================================================

    private void UpdatePlayerExhaustPileCount(int value)
    {
        if (exhaustPileText == null)
            return;


        exhaustPileText.text = value.ToString();

        PlayPunchAnimation(exhaustPileTransform);
    }


    // =====================================================
    // Energy
    // =====================================================

    private void UpdatePlayerEnergy(int _)
    {
        if (energyText == null)
            return;


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
        if (target == null)
            return;


        StartCoroutine(PunchRoutine(target));
    }


    private IEnumerator PunchRoutine(Transform target)
    {
        if (target == null)
            yield break;


        Vector3 originalScale = Vector3.one;
        Vector3 enlargedScale = Vector3.one * 1.1f;

        float duration = 0.08f;
        float t = 0f;


        // 커짐
        while (t < duration)
        {
            if (target == null)
                yield break;


            t += Time.deltaTime;

            target.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    t / duration
                );

            yield return null;
        }


        t = 0f;


        // 원래 크기로
        while (t < duration)
        {
            if (target == null)
                yield break;


            t += Time.deltaTime;

            target.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    t / duration
                );

            yield return null;
        }


        if (target != null)
            target.localScale = originalScale;
    }


    // =====================================================
    // 경고 메시지
    // =====================================================

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
            WarningType.NotEnoughEnergy =>
                "I don't have enough energy..",

            WarningType.HandFull =>
                "My hand is full!",

            WarningType.NoCardsToDraw =>
                "There are no cards to draw!",

            _ => ""
        };


        if (warningRoutine != null)
            StopCoroutine(warningRoutine);


        warningRoutine =
            StartCoroutine(
                WarningRoutine(message)
            );
    }


    private IEnumerator WarningRoutine(string message)
    {
        if (warningBubble == null)
            yield break;


        warningBubble.SetActive(true);


        warningTween?.Kill();


        SetWarningMessage(message);


        RectTransform rect = warningBubbleRect;

        if (rect == null)
            yield break;


        // 초기 상태
        rect.localScale = Vector3.one * 0.8f;


        // 등장 애니메이션
        warningTween =
            rect.DOScale(
                1.15f,
                0.15f
            ).SetEase(Ease.OutBack);


        warningTween =
            rect.DOShakeAnchorPos(
                2.5f,
                7f,
                10,
                90f
            );


        yield return new WaitForSeconds(1.4f);


        // 종료 애니메이션
        warningTween =
            rect.DOScale(
                0.8f,
                0.15f
            ).SetEase(Ease.InBack);


        yield return new WaitForSeconds(0.15f);


        if (warningBubble != null)
            warningBubble.SetActive(false);
    }


    private void SetWarningMessage(string message)
    {
        if (warningText == null)
            return;


        warningText.text = message;


        Canvas.ForceUpdateCanvases();

        warningText.ForceMeshUpdate();


        LayoutRebuilder.ForceRebuildLayoutImmediate(
            warningText.rectTransform
        );


        if (warningBubbleRect != null)
        {
            warningBubbleRect.sizeDelta =
                new Vector2(
                    warningText.preferredWidth - 80f,
                    warningText.preferredHeight - 20f
                );
        }
    }


    // =====================================================
    // 현재 턴 표시
    // =====================================================

    public void ShowTurnBanner(
        int turnCount,
        BattleManager.BattlePhase phase)
    {
        if (turnBanner == null)
            return;


        turnBanner.ShowTurn(
            turnCount,
            phase
        );
    }
}