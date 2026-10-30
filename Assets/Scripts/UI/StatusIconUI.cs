
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

        TooltipTrigger trigger = GetComponent<TooltipTrigger>();

        if (trigger != null)
        {
            trigger.SetContent(
                status.Type.ToString(),
                status.Description,
                status.Icon
            );
        }
    }
}
