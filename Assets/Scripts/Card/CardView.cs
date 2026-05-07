using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 손패의 카드 상호작용을 관리하는 class
/// </summary>
public class CardView : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public CardInstance card;

    public Vector2 targetPos;
    public Quaternion targetRot;

    private float moveSpeed = 12f;

    private RectTransform rect;
    private Vector3 originalScale;

    private Transform startParent;
    private Canvas rootCanvas;

    private bool isDragging;
    private Vector2 dragTargetPos;

    private Coroutine scaleRoutine;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        rootCanvas = GetComponentInParent<Canvas>();
    }

    // =====================================================
    // 카드 데이터 연결
    // =====================================================

    public void SetCard(CardInstance cardInstance)
    {
        card = cardInstance;
    }

    // =====================================================
    // Update (이동 + 회전)
    // =====================================================

    private void Update()
    {
        if (isDragging)
        {
            transform.position = Vector2.Lerp(
                transform.position,
                dragTargetPos,
                Time.deltaTime * 15f
            );

            rect.rotation = Quaternion.Lerp(
                rect.rotation,
                Quaternion.identity,
                Time.deltaTime * 12f
            );

            return;
        }

        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            targetPos,
            Time.deltaTime * moveSpeed
        );

        rect.rotation = Quaternion.Lerp(
            rect.rotation,
            targetRot,
            Time.deltaTime * moveSpeed
        );
    }

    // =====================================================
    // Hover
    // =====================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateScale(originalScale * 1.15f, 0.12f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(originalScale, 0.12f);
    }

    private void AnimateScale(Vector3 targetScale, float duration)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleRoutine(targetScale, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t / duration);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    // =====================================================
    // Drag 시작
    // =====================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        startParent = transform.parent;

        transform.SetAsLastSibling();
        transform.SetParent(rootCanvas.transform);

        dragTargetPos = eventData.position;
    }

    // =====================================================
    // Drag 중
    // =====================================================

    public void OnDrag(PointerEventData eventData)
    {
        dragTargetPos = eventData.position;
    }

    // =====================================================
    // Drag 종료
    // =====================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        AnimateScale(originalScale, 0.1f);

        if (!IsPlayArea())
        {
            ReturnToHand();
            return;
        }

        // 에너지 체크
        // TODO : DBTest → GameState로 교체
        if (DBTest.instance.CurrentEnergy < card.currentCost)
        {
            UIManager.instance.ShowEnergyWarning();
            ReturnToHand();
            return;
        }

        // 카드 사용
        GameState state = BattleManager.Instance.state;
        GameStateManager.instance.ApplyAction(state, card);

        HandManager.instance.UpdateLayout();
        Destroy(gameObject);
    }

    // =====================================================
    // 카드 사용 영역 판정
    // =====================================================

    private bool IsPlayArea()
    {
        return transform.position.y > Screen.height * 0.4f;
    }

    // =====================================================
    // 손패 복귀
    // =====================================================

    private void ReturnToHand()
    {
        transform.SetParent(startParent);
        HandManager.instance.UpdateLayout();
    }
}
