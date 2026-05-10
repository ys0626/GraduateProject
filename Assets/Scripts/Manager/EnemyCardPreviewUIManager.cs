using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 적이 사용할 카드 프리뷰 UI
/// (Inspector에서 지정한 위치에 카드 표시)
/// </summary>
public class EnemyCardPreviewUIManager : MonoBehaviour
{
    public static EnemyCardPreviewUIManager instance;

    [SerializeField] private Transform canvasParent;
    [SerializeField] private GameObject cardShowPrefab;
    [SerializeField] private RectTransform targetPoint;

    private GameObject currentCard;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Show(CardInstance card)
    {
        Hide();

        currentCard = Instantiate(cardShowPrefab, canvasParent);

        RectTransform rt = currentCard.GetComponent<RectTransform>();
        CardUI ui = currentCard.GetComponent<CardUI>();
        ui.Setup(card);

        rt.position = targetPoint.position;

        // =====================================================
        // 등장 연출
        // =====================================================

        rt.localScale = Vector3.zero;

        rt.DOScale(0.75f, 0.25f)
            .SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        if (currentCard == null) return;

        RectTransform rt = currentCard.GetComponent<RectTransform>();

        rt.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Destroy(currentCard);
                currentCard = null;
            });
    }
}