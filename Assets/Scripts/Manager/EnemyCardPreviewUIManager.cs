
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 적이 사용할 카드 프리뷰 UI
/// 전투 유형에 따라 표시 위치 변경
/// </summary>
public class EnemyCardPreviewUIManager : MonoBehaviour
{
    public static EnemyCardPreviewUIManager instance;

    [Header("Card Preview")]
    [SerializeField] private Transform canvasParent;
    [SerializeField] private GameObject cardShowPrefab;
    [SerializeField] private RectTransform targetPoint;

    [Header("Normal Position")]
    [SerializeField]
    private Vector2 normalPosition =
        new Vector2(299.3f, 277.4f);

    [Header("Elite Position")]
    [SerializeField]
    private Vector2 elitePosition =
        new Vector2(299.3f, 277.4f);

    [Header("Crystal Falconer Elite Preview")]
    [SerializeField]
    private Vector2 crystalFalconerElitePreviewPosition =
    new Vector2(50f, 120f);

    [Header("Boss Position")]
    [SerializeField]
    private Vector2 bossPosition =
        new Vector2(299.3f, 277.4f);

    private GameObject currentCard;


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

    private void OnDestroy()
    {
        if (currentCard != null)
        {
            currentCard.transform.DOKill();
        }
    }


    // =====================================================
    // 카드 프리뷰 위치 설정
    // =====================================================

    private void ApplyPreviewPosition()
    {
        if (targetPoint == null)
        {
            Debug.LogError(
                "[EnemyCardPreviewUIManager] Target Point가 연결되지 않았습니다.",
                this
            );
            return;
        }

        if (GameData.instance == null)
        {
            Debug.LogError(
                "[EnemyCardPreviewUIManager] GameData가 없습니다.",
                this
            );
            return;
        }

        EnemyData currentEnemy = GameData.instance.CurrentEnemyData;

        // Crystal Falconer 전용 프리뷰 위치
        if (GameData.instance.CurrentBattleType == BattleType.Elite &&
            currentEnemy != null &&
            currentEnemy.enemyName == "Crystal Falconer")
        {
            targetPoint.anchoredPosition =
                crystalFalconerElitePreviewPosition;

            return;
        }

        // 일반적인 전투 유형별 프리뷰 위치
        switch (GameData.instance.CurrentBattleType)
        {
            case BattleType.Normal:
                targetPoint.anchoredPosition = normalPosition;
                break;

            case BattleType.Elite:
                targetPoint.anchoredPosition = elitePosition;
                break;

            case BattleType.Boss:
                targetPoint.anchoredPosition = bossPosition;
                break;
        }
    }


    // =====================================================
    // 카드 프리뷰 표시
    // =====================================================

    public void Show(CardInstance card)
    {
        if (card == null || cardShowPrefab == null || canvasParent == null)
        {
            Debug.LogError(
                "[EnemyCardPreviewUIManager] 카드 프리뷰 설정이 올바르지 않습니다.",
                this
            );

            return;
        }

        Hide();

        ApplyPreviewPosition();

        currentCard = Instantiate(cardShowPrefab, canvasParent);

        RectTransform rt = currentCard.GetComponent<RectTransform>();

        CardUI ui = currentCard.GetComponent<CardUI>();

        if (rt == null || ui == null)
        {
            Debug.LogError(
                "[EnemyCardPreviewUIManager] Card Prefab의 RectTransform 또는 CardUI가 없습니다.",
                this
            );

            Destroy(currentCard);
            currentCard = null;

            return;
        }

        ui.Setup(card);

        rt.position = targetPoint.position;

        // 등장 연출
        rt.localScale = Vector3.zero;

        rt.DOScale(0.75f, 0.25f)
            .SetEase(Ease.OutBack)
            .SetLink(currentCard);
    }


    // =====================================================
    // 카드 프리뷰 제거
    // =====================================================

    public void Hide()
    {
        if (currentCard == null)
        {
            return;
        }

        // 제거할 카드를 별도로 보관
        GameObject cardToHide = currentCard;

        currentCard = null;

        RectTransform rt =
            cardToHide.GetComponent<RectTransform>();

        if (rt == null)
        {
            Destroy(cardToHide);

            return;
        }

        rt.DOKill();

        rt.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .SetLink(cardToHide)
            .OnComplete(() =>
            {
                if (cardToHide != null)
                {
                    Destroy(cardToHide);
                }
            });
    }
}
