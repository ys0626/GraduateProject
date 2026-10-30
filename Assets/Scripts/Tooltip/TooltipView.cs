using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipView : MonoBehaviour
{
    [Header("Tooltip UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    public void SetContent(
        string title,
        string description,
        Sprite icon)
    {
        bool hasIcon = icon != null;
        bool hasTitle = !string.IsNullOrEmpty(title);
        bool hasDescription = !string.IsNullOrEmpty(description);

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);

            if (hasIcon)
                iconImage.sprite = icon;
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(hasTitle);

            if (hasTitle)
                titleText.text = title;
        }

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(hasDescription);

            if (hasDescription)
                descriptionText.text = description;
        }
    }
}