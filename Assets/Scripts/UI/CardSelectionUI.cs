
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSelectionUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Card UI")]
    [SerializeField] private CardUI cardUI;

    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float animationDuration = 0.12f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private Action<CardData> onCardClicked;

    private bool isInitialized;
    private bool isInteractable = true;

    private void Awake()
    {
        if (cardUI == null)
        {
            cardUI = GetComponent<CardUI>();
        }

        originalScale = transform.localScale;
    }

    public void Setup(
        CardData cardData,
        Action<CardData> clickCallback)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[CardSelectionUI] CardData가 null입니다.",
                this
            );

            gameObject.SetActive(false);
            return;
        }

        if (cardUI == null)
        {
            cardUI = GetComponent<CardUI>();
        }

        if (cardUI == null)
        {
            Debug.LogError(
                "[CardSelectionUI] CardUI 컴포넌트가 없습니다.",
                this
            );

            gameObject.SetActive(false);
            return;
        }

        // 표시용 카드 인스턴스 생성
        CardInstance displayCard =
            new CardInstance(cardData);

        cardUI.Setup(displayCard);

        onCardClicked = clickCallback;

        isInteractable = true;
        isInitialized = true;

        transform.localScale = originalScale;
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

        if (!interactable)
        {
            StopScaleAnimation();
            transform.localScale = originalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanInteract())
        {
            return;
        }

        AnimateScale(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanInteract())
        {
            return;
        }

        CardInstance displayedCard = cardUI.GetCard();

        if (displayedCard == null ||
            displayedCard.data == null)
        {
            return;
        }

        onCardClicked?.Invoke(displayedCard.data);
    }

    private bool CanInteract()
    {
        return isInitialized &&
               isInteractable &&
               gameObject.activeInHierarchy;
    }

    private void AnimateScale(Vector3 targetScale)
    {
        StopScaleAnimation();

        scaleCoroutine = StartCoroutine(
            ScaleRoutine(targetScale)
        );
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, animationDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                t
            );

            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }

    private void StopScaleAnimation()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopScaleAnimation();

        transform.localScale = originalScale;
    }
}
