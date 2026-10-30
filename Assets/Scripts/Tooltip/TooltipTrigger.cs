using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Tooltip Content")]
    [SerializeField] private string tooltipTitle;

    [TextArea(2, 5)]
    [SerializeField] private string tooltipDescription;

    [SerializeField] private Sprite tooltipIcon;

    [Header("Tooltip Position")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0f, 50f);

    [Header("Tooltip Size")]
    [SerializeField] private bool useCustomSize = true;
    [SerializeField] private Vector2 tooltipSize = new Vector2(250f, 120f);

    private RectTransform targetRect;

    private void Awake()
    {
        targetRect = GetComponent<RectTransform>();
    }

    public void SetContent(
        string title,
        string description,
        Sprite icon = null)
    {
        tooltipTitle = title;
        tooltipDescription = description;
        tooltipIcon = icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
            return;

        TooltipManager.Instance.Show(
            tooltipTitle,
            tooltipDescription,
            tooltipIcon,
            targetRect,
            positionOffset,
            useCustomSize,
            tooltipSize
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
            return;

        TooltipManager.Instance.Hide();
    }
}