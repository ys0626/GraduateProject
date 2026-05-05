using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 손 패의 카드들의 상호작용을 관리하는 class 
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
            // 드래그 중 위치 추적
            transform.position = Vector2.Lerp(
                transform.position,
                dragTargetPos,
                Time.deltaTime * 15f
            );

            // 정면으로 부드럽게 회전
            rect.rotation = Quaternion.Lerp(
                rect.rotation,
                Quaternion.identity,
                Time.deltaTime * 12f
            );

            return;
        }

        // 손패 위치 복귀
        rect.anchoredPosition =
            Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);

        rect.rotation =
            Quaternion.Lerp(rect.rotation, targetRot, Time.deltaTime * moveSpeed);
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

        transform.SetAsLastSibling();

        startParent = transform.parent;

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
    // Drag 종료 (핵심 판정 로직)
    // =====================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        AnimateScale(originalScale, 0.1f);

        //마우스의 Y 위치로 카드의 사용 여부 판단
        if (IsPlayArea())
        {
            //사용 가능한 에너지 있는 지 체크
            if (DBTest.instance.CurrentEnergy < card.currentCost)
            {
                //에너지 부족 말풍선 출력
                UIManager.instance.ShowEnergyWarning();

                // 손패로 복귀
                transform.SetParent(startParent);
                HandManager.instance.UpdateLayout();

                return;
            }

            // =====================================================
            // 카드 사용
            // =====================================================
            
            //1. 카드의 효과 생성
            ICardEffect effect = CardEffectFactory.Create(card);

            //2. 카드 실행
            effect?.Execute();

            //3. 카드 처리
            DeckManager.instance.DiscardCard(card);
            HandManager.instance.RemoveCard(this);

            Destroy(gameObject);
            return;
        }

        transform.SetParent(startParent);

        HandManager.instance.UpdateLayout();
    }

    // =====================================================
    // 카드 사용 영역 판정
    // =====================================================

    private bool IsPlayArea()
    {
        return transform.position.y > Screen.height * 0.4f;
    }
}