using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private TMP_Text costText;

    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private Image sprite;

    private CardInstance card;

    public void Setup(CardInstance cardInstance)
    {
        card = cardInstance;

        RefreshUI();
    }

    public void RefreshUI()
    {
        nameText.text =
            card.data.cardName;

        costText.text =
            card.currentCost.ToString();

        descriptionText.text =
            card.data.description;

        sprite.sprite =
            card.data.artwork;
    }

    public CardInstance GetCard()
    {
        return card;
    }
}