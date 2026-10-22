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

    // 기존 팝업에서 사용하는 콜백
    private Action<CardData> onCardClicked;

    // 강화 팝업에서 사용하는 콜백
    private Action<CardSelectionUI, CardInstance> onUpgradeCardClicked;

    private bool isInitialized;
    private bool isInteractable = true;
    private bool isClickEnabled = true;

    // 강화 팝업 전용 데이터
    private bool isUpgradeMode;
    private bool isSelected;
    private bool isPointerOver;

    // 실제 덱의 카드 인스턴스
    private CardInstance originalCardInstance;

    private void Awake()
    {
        if (cardUI == null)
        {
            cardUI = GetComponent<CardUI>();
        }

        originalScale = transform.localScale;
    }

    // 기존 CardData 기반 Setup
    // RewardPanelUI.cs와의 호환성을 위해 유지
    public void Setup(CardData cardData, Action<CardData> clickCallback)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[CardSelectionUI] CardData가 null입니다.", this);

            gameObject.SetActive(false);
            return;
        }

        CardInstance displayCard = new CardInstance(cardData);
        SetupInternal(displayCard, clickCallback);
    }

    // 기존 CardInstance 기반 Setup
    // 일반 카드 더미 팝업과의 호환성을 위해 유지
    public void Setup(
        CardInstance cardInstance,
        Action<CardData> clickCallback)
    {
        if (cardInstance == null || cardInstance.data == null)
        {
            Debug.LogWarning(
                "[CardSelectionUI] CardInstance가 null입니다.", this);

            gameObject.SetActive(false);
            return;
        }

        SetupInternal(cardInstance, clickCallback);
    }

    // 강화 팝업 전용 Setup
    public void SetupForUpgrade(
        CardInstance cardInstance,
        Action<CardSelectionUI, CardInstance> clickCallback)
    {
        if (cardInstance == null || cardInstance.data == null)
        {
            Debug.LogWarning(
                "[CardSelectionUI] 강화할 CardInstance가 null입니다.",
                this);

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
                this);

            gameObject.SetActive(false);
            return;
        }

        // 실제 덱의 인스턴스를 보관한다.
        // 여기서는 Upgrade()를 호출하지 않는다.
        originalCardInstance = cardInstance;

        onCardClicked = null;
        onUpgradeCardClicked = clickCallback;

        isUpgradeMode = true;
        isSelected = false;
        isPointerOver = false;

        isInteractable = true;
        isClickEnabled = true;
        isInitialized = true;

        transform.localScale = originalScale;

        // 최초에는 일반 카드 상태로 표시한다.
        cardUI.Setup(originalCardInstance);
    }

    private void SetupInternal(
        CardInstance displayCard,
        Action<CardData> clickCallback)
    {
        if (cardUI == null)
        {
            cardUI = GetComponent<CardUI>();
        }

        if (cardUI == null)
        {
            Debug.LogError(
                "[CardSelectionUI] CardUI 컴포넌트가 없습니다.",
                this);

            gameObject.SetActive(false);
            return;
        }

        originalCardInstance = null;
        onUpgradeCardClicked = null;

        isUpgradeMode = false;
        isSelected = false;
        isPointerOver = false;

        cardUI.Setup(displayCard);

        onCardClicked = clickCallback;

        isInteractable = true;
        isClickEnabled = true;
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

    public void SetClickEnabled(bool enabled)
    {
        isClickEnabled = enabled;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanInteract())
        {
            return;
        }

        isPointerOver = true;

        AnimateScale(originalScale * hoverScale);

        if (isUpgradeMode && !isSelected)
        {
            ShowUpgradedPreview();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (isUpgradeMode)
        {
            // 선택된 카드는 마우스가 나가도 미리보기를 유지한다.
            if (!isSelected)
            {
                ShowOriginalCard();
            }
        }

        AnimateScale(originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanInteract() || !isClickEnabled)
        {
            return;
        }

        if (isUpgradeMode)
        {
            HandleUpgradeCardClick();
            return;
        }

        // 기존 일반 팝업 동작
        CardInstance displayedCard = cardUI.GetCard();

        if (displayedCard == null || displayedCard.data == null)
        {
            return;
        }

        onCardClicked?.Invoke(displayedCard.data);
    }

    private void HandleUpgradeCardClick()
    {
        if (originalCardInstance == null ||
            originalCardInstance.data == null)
        {
            return;
        }

        // 선택 상태를 전환한다.
        isSelected = !isSelected;

        if (isSelected)
        {
            ShowUpgradedPreview();
        }
        else
        {
            if (isPointerOver)
            {
                ShowUpgradedPreview();
            }
            else
            {
                ShowOriginalCard();
            }
        }

        // CardPopupManager가 현재 선택된 카드를 관리한다.
        onUpgradeCardClicked?.Invoke(
            this,
            isSelected ? originalCardInstance : null);
    }

    private void ShowUpgradedPreview()
    {
        if (originalCardInstance == null ||
            originalCardInstance.data == null)
        {
            return;
        }

        // 실제 덱 카드를 변경하지 않도록 복제본을 만든다.
        CardInstance previewCard =
            new CardInstance(originalCardInstance);

        // 이미 강화된 카드는 강화하지 않는다.
        if (!previewCard.upgraded)
        {
            previewCard.Upgrade();
        }

        cardUI.Setup(previewCard);
    }

    private void ShowOriginalCard()
    {
        if (originalCardInstance == null ||
            originalCardInstance.data == null)
        {
            return;
        }

        // 원본을 다시 표시한다. 원본의 강화 여부와 비용을 유지한다.
        cardUI.Setup(originalCardInstance);
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void SetUpgradeSelected(bool selected)
    {
        if (!isUpgradeMode)
            return;

        isSelected = selected;

        if (isSelected || isPointerOver)
        {
            ShowUpgradedPreview();
        }
        else
        {
            ShowOriginalCard();
        }
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
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
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

            transform.localScale =
                Vector3.Lerp(startScale, targetScale, t);

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

        isPointerOver = false;
        isSelected = false;
    }
}