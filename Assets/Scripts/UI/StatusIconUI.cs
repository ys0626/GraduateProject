
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text valueText;

    [Header("Tooltip Prefabs")]
    [SerializeField] private TooltipView defaultTooltipPrefab;
    [SerializeField] private TooltipView blockRetentionTooltipPrefab;
    [SerializeField] private TooltipView strengthPerTurnTooltipPrefab;

    private TooltipTrigger tooltipTrigger;

    private void Awake()
    {
        tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    public void Setup(Status status)
    {
        iconImage.sprite = status.Icon;

        valueText.text =
            status.Type == StatusType.BlockRetention
            ? ""
            : status.Value.ToString();

        if (tooltipTrigger != null)
        {
            string statusName = status.Type switch
            {
                StatusType.Vulnerable => "취약",
                StatusType.Dexterity => "민첩",
                StatusType.Strength => "힘",
                StatusType.Weak => "약화",
                StatusType.BlockRetention => "방어도 유지",
                StatusType.StrengthPerTurn => "힘 축적",
                _ => status.Type.ToString()
            };

            tooltipTrigger.SetContent(
                statusName,
                status.Description,
                status.Icon
            );

            TooltipView selectedPrefab = status.Type switch
            {
                StatusType.BlockRetention => blockRetentionTooltipPrefab,
                StatusType.StrengthPerTurn => strengthPerTurnTooltipPrefab,
                _ => defaultTooltipPrefab
            };

            tooltipTrigger.SetTooltipPrefab(selectedPrefab);
        }
    }
}
