
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text valueText;

    private TooltipTrigger tooltipTrigger;

    private void Awake()
    {
        tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    public void Setup(Status status)
    {
        iconImage.sprite = status.Icon;

        valueText.text = status.Value.ToString();

        if (tooltipTrigger != null)
        {
            string statusName = status.Type switch
            {
                StatusType.Vulnerable => "취약",
                StatusType.Dexterity => "민첩",
                StatusType.Strength => "힘",
                StatusType.Weak => "약화",
                _ => status.Type.ToString()
            };

            tooltipTrigger.SetContent(
                statusName,
                status.Description,
                status.Icon
            );
        }
    }
}
