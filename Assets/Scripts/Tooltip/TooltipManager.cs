using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Screen")]
    [SerializeField] private Vector2 screenPadding = new Vector2(12f, 12f);

    private Canvas parentCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        if (tooltipRect != null)
            parentCanvas = tooltipRect.GetComponentInParent<Canvas>();

        if (parentCanvas != null)
            canvasRect = parentCanvas.transform as RectTransform;
    }

    public void Show(
        string title,
        string description,
        Sprite icon,
        RectTransform target,
        Vector2 positionOffset,
        bool useCustomSize,
        Vector2 customSize)
    {
        if (tooltipPanel == null || tooltipRect == null || target == null)
            return;

        // 이름 표시
        bool hasTitle = !string.IsNullOrEmpty(title);

        if (titleText != null)
        {
            titleText.gameObject.SetActive(hasTitle);

            if (hasTitle)
                titleText.text = title;
        }

        // 설명 표시
        bool hasDescription = !string.IsNullOrEmpty(description);

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(hasDescription);

            if (hasDescription)
                descriptionText.text = description;
        }

        // 아이콘 표시
        bool hasIcon = icon != null;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);

            if (hasIcon)
                iconImage.sprite = icon;
        }

        // 오브젝트별 툴팁 크기
        if (useCustomSize)
        {
            tooltipRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                customSize.x
            );

            tooltipRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                customSize.y
            );
        }

        tooltipPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();

        UpdatePosition(target, positionOffset);
    }

    public void Hide()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void UpdatePosition(
        RectTransform target,
        Vector2 positionOffset)
    {
        if (parentCanvas == null || canvasRect == null)
            return;

        // 대상 오브젝트의 중심 월드 좌표
        Vector3 targetWorldPosition =
            target.TransformPoint(target.rect.center);

        // 대상 오브젝트의 중심을 화면 좌표로 변환
        Camera canvasCamera = null;

        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = parentCanvas.worldCamera;

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            canvasCamera,
            targetWorldPosition
        );

        // 오브젝트 중심에 오프셋 적용
        screenPosition += positionOffset;

        // 화면 밖으로 나가지 않도록 보정
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

        // 화면 좌표를 Canvas 로컬 좌표로 변환
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
            Instance = null;
    }
}