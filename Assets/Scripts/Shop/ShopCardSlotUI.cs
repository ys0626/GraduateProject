
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardSlotUI : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardSelectionUI cardPrefab;

    [Header("Price")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image goldIcon;

    private CardSelectionUI spawnedCard;
    private bool isPurchased;

    public bool IsPurchased => isPurchased;

    public void Setup(
        CardData cardData,
        int price,
        Action<CardData> onBuy)
    {
        isPurchased = false;

        if (cardData == null ||
            cardContainer == null ||
            cardPrefab == null)
        {
            Debug.LogError(
                "[ShopCardSlotUI] 카드 설정이 올바르지 않습니다.",
                this);
            return;
        }

        if (spawnedCard != null)
        {
            Destroy(spawnedCard.gameObject);
        }

        spawnedCard = Instantiate(
            cardPrefab,
            cardContainer
        );

        spawnedCard.transform.localScale = Vector3.one;

        spawnedCard.Setup(
            cardData,
            selectedCard => onBuy?.Invoke(selectedCard)
        );

        spawnedCard.gameObject.SetActive(true);

        if (priceText != null)
        {
            priceText.text = price.ToString();
            priceText.gameObject.SetActive(true);
        }

        if (goldIcon != null)
        {
            goldIcon.gameObject.SetActive(true);
        }
    }

    public void MarkPurchased()
    {
        if (isPurchased)
        {
            return;
        }

        isPurchased = true;

        // 카드 UI만 숨김
        if (spawnedCard != null)
        {
            spawnedCard.gameObject.SetActive(false);
        }

        // 가격 숫자만 숨김
        if (priceText != null)
        {
            priceText.text = "";
            priceText.gameObject.SetActive(false);
        }

        // 골드 아이콘 숨김
        if (goldIcon != null)
        {
            goldIcon.gameObject.SetActive(false);
        }
    }
}
