using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("Tooltip Root")]
    [SerializeField] private RectTransform tooltipRoot;

    [Header("Screen")]
    [SerializeField] private Vector2 screenPadding = new Vector2(12f, 12f);

    private Canvas parentCanvas;
    private RectTransform canvasRect;

    private TooltipView currentTooltip;
    private TooltipTrigger currentTrigger;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tooltipRoot != null)
        {
            parentCanvas = tooltipRoot.GetComponentInParent<Canvas>();

            if (parentCanvas != null)
                canvasRect = parentCanvas.transform as RectTransform;
        }
    }

    public void Show(TooltipTrigger trigger)
    {
        if (trigger == null || trigger.TooltipPrefab == null)
            return;

        if (tooltipRoot == null || parentCanvas == null)
            return;

        HideCurrent();

        currentTrigger = trigger;

        currentTooltip = Instantiate(
            trigger.TooltipPrefab,
            tooltipRoot
        );

        currentTooltip.SetContent(
            trigger.TooltipTitle,
            trigger.TooltipDescription,
            trigger.TooltipIcon
        );

        Canvas.ForceUpdateCanvases();

        UpdatePosition(
            trigger.TargetRect,
            trigger.PositionOffset
        );

        currentTooltip.transform.SetAsLastSibling();
    }

    public void Hide(TooltipTrigger trigger)
    {
        if (currentTrigger != trigger)
            return;

        HideCurrent();
    }

    private void HideCurrent()
    {
        if (currentTooltip != null)
            Destroy(currentTooltip.gameObject);

        currentTooltip = null;
        currentTrigger = null;
    }

    private void UpdatePosition(
        RectTransform target,
        Vector2 positionOffset)
    {
        if (target == null || currentTooltip == null)
            return;

        Camera canvasCamera = null;

        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = parentCanvas.worldCamera;

        Vector3 targetWorldPosition =
            target.TransformPoint(target.rect.center);

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            canvasCamera,
            targetWorldPosition
        );

        screenPosition += positionOffset;

        RectTransform tooltipRect =
            currentTooltip.transform as RectTransform;

        Vector2 tooltipSize = tooltipRect.rect.size;

        float halfWidth = tooltipSize.x * 0.5f;
        float halfHeight = tooltipSize.y * 0.5f;

        screenPosition.x = Mathf.Clamp(
            screenPosition.x,
            screenPadding.x + halfWidth,
            Screen.width - screenPadding.x - halfWidth
        );

        screenPosition.y = Mathf.Clamp(
            screenPosition.y,
            screenPadding.y + halfHeight,
            Screen.height - screenPadding.y - halfHeight
        );

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvasCamera,
            out Vector2 localPosition))
        {
            tooltipRect.position = canvasRect.TransformPoint(localPosition);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            HideCurrent();
            Instance = null;
        }
    }
}