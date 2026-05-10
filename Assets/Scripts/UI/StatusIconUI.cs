using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text valueText;

    public void Setup(Status status)
    {
        iconImage.sprite = status.Icon;

        valueText.text = status.Value.ToString();
    }
}