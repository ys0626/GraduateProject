using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Tooltip Prefab")]
    [SerializeField] private TooltipView tooltipPrefab;

    [Header("Tooltip Content")]
    [SerializeField] private string tooltipTitle;

    [TextArea(2, 5)]
    [SerializeField] private string tooltipDescription;

    [SerializeField] private Sprite tooltipIcon;

    [Header("Tooltip Position")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0f, 50f);

    public TooltipView TooltipPrefab => tooltipPrefab;
    public string TooltipTitle => tooltipTitle;
    public string TooltipDescription => tooltipDescription;
    public Sprite TooltipIcon => tooltipIcon;
    public Vector2 PositionOffset => positionOffset;

    public RectTransform TargetRect { get; private set; }

    private void Awake()
    {
        TargetRect = GetComponent<RectTransform>();
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

    public void SetTooltipPrefab(TooltipView prefab)
    {
        tooltipPrefab = prefab;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
            return;

        TooltipManager.Instance.Show(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    public void HideTooltip()
    {
        if (TooltipManager.Instance == null)
            return;

        TooltipManager.Instance.Hide(this);
    }
}