using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

    [Header("Enemy HP Gauge")]
    [SerializeField] private TMP_Text enemyHPBarText;
    [SerializeField] private Transform enemyHPBarTextTransform;
    [SerializeField] private EnemyHPBarUI enemyHPBarUI;

    /// <summary>
    /// //카드 사용에 필요한 에너지 부족 시 말풍선 출력
    /// </summary>
    [Header("Energy Warning UI")]   
    public GameObject energyWarningBubble;
    private Coroutine warningRoutine;

    [Header("HP Gauge")]
    GameObject playerHPGauge;
    GameObject enemyHPGauge;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnDisable()
    {
        // 플레이어 HP
        DBTest.instance.OnCurrentHPChanged -= UpdateHP;
        DBTest.instance.OnMaxHPChanged -= UpdateHP;

        //적 HP
        DBTest.instance.OnEnemyCurrentHPChanged -= UpdateEnemyHP;
        DBTest.instance.OnEnemyMaxHPChanged -= UpdateEnemyHP;

        // Gold
        DBTest.instance.OnGoldChanged -= UpdateGold;

        // Deck
        DBTest.instance.OnDeckCountChanged -= UpdateDeckCount;

        // Draw Pile
        DBTest.instance.OnDrawPileCountChanged -= UpdateDrawPileCount;

        // Discard Pile
        DBTest.instance.OnDiscardPileCountChanged -= UpdateDiscardPileCount;

        // Exhaust Pile
        DBTest.instance.OnExhaustPileCountChanged -= UpdateExhaustPileCount;

        // Energy
        DBTest.instance.OnCurrentEnergyChanged -= UpdateEnergy;
        DBTest.instance.OnMaxEnergyChanged -= UpdateEnergy;
    }

    private void Start()
    {
        DBTest.instance.OnCurrentHPChanged += UpdateHP;
        DBTest.instance.OnMaxHPChanged += UpdateHP;
        DBTest.instance.OnEnemyCurrentHPChanged += UpdateEnemyHP;
        DBTest.instance.OnEnemyMaxHPChanged += UpdateEnemyHP;


        DBTest.instance.OnGoldChanged += UpdateGold;
        DBTest.instance.OnDeckCountChanged += UpdateDeckCount;
        DBTest.instance.OnDrawPileCountChanged += UpdateDrawPileCount;
        DBTest.instance.OnDiscardPileCountChanged += UpdateDiscardPileCount;
        DBTest.instance.OnExhaustPileCountChanged += UpdateExhaustPileCount;
        DBTest.instance.OnCurrentEnergyChanged += UpdateEnergy;
        DBTest.instance.OnMaxEnergyChanged += UpdateEnergy;

        UpdateHP(0);

        UpdateEnemyHP(0);

        UpdateGold(DBTest.instance.Gold);

        UpdateDeckCount(DBTest.instance.DeckCount);

        UpdateDrawPileCount(DBTest.instance.DrawPileCount);

        UpdateDiscardPileCount(DBTest.instance.DiscardPileCount);

        UpdateExhaustPileCount(DBTest.instance.ExhaustPileCount);

        UpdateEnergy(0);
    }

    // =====================================================
    // 플레이어 HP
    // =====================================================

    private void UpdateHP(int _)
    {
        hpText.text =
            DBTest.instance.CurrentHP +
            "/" +
            DBTest.instance.MaxHP;

        hpBarText.text =
            DBTest.instance.CurrentHP +
            "/" +
            DBTest.instance.MaxHP;

        PlayPunchAnimation(hpTransform);
        PlayPunchAnimation(hpBarTextTransform);

        playerHPBarUI.RefreshPlayerHPBar();
    }

    // =====================================================
    // 적 HP
    // =====================================================
    private void UpdateEnemyHP(int _)
    {
        enemyHPBarText.text =
            DBTest.instance.EnemyCurrentHP +
            "/" +
            DBTest.instance.EnemyMaxHP;

        PlayPunchAnimation(enemyHPBarTextTransform);

        enemyHPBarUI.RefreshEnemyHPBar();
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

    private void UpdateDeckCount(int value)
    {
        deckCountText.text = value.ToString();

        PlayPunchAnimation(deckCountTransform);
    }

    // =====================================================
    // Draw Pile
    // =====================================================

    private void UpdateDrawPileCount(int value)
    {
        drawPileText.text = value.ToString();

        PlayPunchAnimation(drawPileTransform);
    }

    // =====================================================
    // Discard Pile
    // =====================================================

    private void UpdateDiscardPileCount(int value)
    {
        discardPileText.text = value.ToString();

        PlayPunchAnimation(discardPileTransform);
    }

    // =====================================================
    // Exhaust Pile
    // =====================================================

    private void UpdateExhaustPileCount(int value)
    {
        exhaustPileText.text = value.ToString();

        PlayPunchAnimation(exhaustPileTransform);
    }

    // =====================================================
    // Energy
    // =====================================================

    private void UpdateEnergy(int _)
    {
        energyText.text =
            DBTest.instance.CurrentEnergy +
            "/" +
            DBTest.instance.MaxEnergy;

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
    // 카드 사용에 필요한 에너지 부족 시 말풍선 출력
    // =====================================================
    public void ShowEnergyWarning()
    {
        if (warningRoutine != null)
            StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(EnergyWarningRoutine());
    }

    private IEnumerator EnergyWarningRoutine()
    {
        energyWarningBubble.SetActive(true);

        RectTransform rect = energyWarningBubble.GetComponent<RectTransform>();

        rect.DOShakeAnchorPos(
            2.5f,   //duration
            7f,     //strength
            10,     //vibrato
            90f     //randomness
        );

        yield return new WaitForSeconds(1.4f);

        energyWarningBubble.SetActive(false);
    }


}
