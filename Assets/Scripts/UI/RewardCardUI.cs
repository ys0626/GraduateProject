using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private Image cardIcon;
    [SerializeField] private Button button;

    private CardData cardData;
    private RewardPanelUI rewardPanel;


    public void Setup(CardData card, RewardPanelUI panel)
    {
        cardData = card;
        rewardPanel = panel;

        if (cardData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }

        if (cardIcon != null)
        {
            cardIcon.sprite = cardData.artwork;
            cardIcon.enabled = cardData.artwork != null;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = true;
            button.onClick.AddListener(OnClicked);
        }
    }


    private void OnClicked()
    {
        if (rewardPanel == null)
        {
            return;
        }

        rewardPanel.SelectCard(cardData);
    }
}