
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button iconButton;
    [SerializeField] private Image goldIcon;

    private bool isPurchased;
    private Action onBuy;

    public bool IsPurchased => isPurchased;

    private void Awake()
    {
        if (iconButton != null)
        {
            iconButton.onClick.AddListener(OnIconClicked);
        }
    }

    public void Setup(
        Sprite icon,
        string itemName,
        string description,
        int price,
        Action buyCallback)
    {
        isPurchased = false;
        onBuy = buyCallback;

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.preserveAspect = true;
            itemIcon.enabled = icon != null;
            itemIcon.gameObject.SetActive(icon != null);
        }

        if (priceText != null)
        {
            priceText.text = price.ToString();
            priceText.gameObject.SetActive(true);
        }

        if (goldIcon != null)
        {
            goldIcon.gameObject.SetActive(true);
        }

        if (iconButton != null)
        {
            iconButton.interactable = true;
        }
    }

    private void OnIconClicked()
    {
        if (isPurchased)
        {
            return;
        }

        onBuy?.Invoke();
    }

    public void MarkPurchased()
    {
        if (isPurchased)
        {
            return;
        }

        isPurchased = true;

        if (iconButton != null)
        {
            iconButton.interactable = false;
        }

        if (itemIcon != null)
        {
            itemIcon.enabled = false;
            itemIcon.gameObject.SetActive(false);
        }

        if (priceText != null)
        {
            priceText.text = "";
            priceText.gameObject.SetActive(false);
        }

        if (goldIcon != null)
        {
            goldIcon.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (iconButton != null)
        {
            iconButton.onClick.RemoveListener(OnIconClicked);
        }
    }
}
