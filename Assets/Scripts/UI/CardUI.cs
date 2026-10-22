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

    [Header("Upgrade Colors")]
    [SerializeField] private Color normalNameColor = Color.white;

    [SerializeField]
    private Color upgradedNameColor =
        new Color(0.22f, 0.66f, 0.42f);

    private CardInstance card;

    public void Setup(CardInstance cardInstance)
    {
        card = cardInstance;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (card == null || card.data == null)
        {
            return;
        }

        nameText.text = card.GetDisplayName();

        nameText.color = card.upgraded
            ? upgradedNameColor
            : normalNameColor;

        costText.text = card.currentCost.ToString();

        costText.color =
            card.upgraded &&
            card.data.upgradeType == CardUpgradeType.CostReduction
                ? upgradedNameColor
                : normalNameColor;

        descriptionText.text = card.GetDisplayDescription();

        sprite.sprite = card.data.artwork;
    }

    public CardInstance GetCard()
    {
        return card;
    }
}