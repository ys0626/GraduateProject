
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPanelUI : MonoBehaviour
{
    [Header("Reward Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform rewardContent;
    [SerializeField] private RewardItemUI rewardItemPrefab;

    [Header("Card Choice Panel")]
    [SerializeField] private GameObject cardChoicePanel;
    [SerializeField] private RewardCardUI[] cardChoices;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    private readonly List<Reward> currentRewards =
        new List<Reward>();

    private readonly List<RewardItemUI> spawnedItems =
        new List<RewardItemUI>();

    private RewardItemUI cardRewardItem;

    private bool cardChoiceOpen;
    private bool cardSelected;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                OnContinueClicked
            );

            continueButton.onClick.AddListener(
                OnContinueClicked
            );
        }

        Hide();
    }

    public void ShowRewards()
    {
        if (RewardManager.Instance == null)
        {
            Debug.LogError(
                "RewardManager.Instance가 없습니다."
            );

            return;
        }

        ClearRewardItems();

        currentRewards.Clear();

        currentRewards.AddRange(
            RewardManager.Instance.GenerateRewards()
        );

        cardRewardItem = null;
        cardChoiceOpen = false;
        cardSelected = false;

        if (cardChoicePanel != null)
        {
            cardChoicePanel.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        DisplayRewards();
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (cardChoicePanel != null)
        {
            cardChoicePanel.SetActive(false);
        }

        cardChoiceOpen = false;
    }

    private void DisplayRewards()
    {
        if (rewardContent == null ||
            rewardItemPrefab == null)
        {
            Debug.LogError(
                "Reward Content 또는 Reward Item Prefab이 연결되지 않았습니다."
            );

            return;
        }

        bool cardRewardDisplayed = false;

        for (int i = 0; i < currentRewards.Count; i++)
        {
            Reward reward = currentRewards[i];

            if (reward == null)
            {
                continue;
            }

            switch (reward.RewardType)
            {
                case RewardType.Gold:
                    CreateRewardItem(
                        reward,
                        "+" + reward.GoldAmount + " Gold"
                    );
                    break;

                case RewardType.Cheese:
                    CreateRewardItem(
                        reward,
                        "Cheese +" +
                        reward.CheeseHealAmount +
                        " HP"
                    );
                    break;

                case RewardType.Card:
                    // 여러 카드 선택지를 하나의 항목으로 표시
                    if (!cardRewardDisplayed)
                    {
                        cardRewardItem = CreateRewardItem(
                            reward,
                            "카드 획득"
                        );

                        cardRewardDisplayed = true;
                    }
                    break;

                case RewardType.Relic:
                    // 유물 시스템 구현 전까지 표시하지 않음
                    break;
            }
        }
    }

    private RewardItemUI CreateRewardItem(
        Reward reward,
        string displayName)
    {
        RewardItemUI item = Instantiate(
            rewardItemPrefab,
            rewardContent
        );

        item.Setup(
            reward,
            displayName,
            this
        );

        spawnedItems.Add(item);

        return item;
    }

    // RewardItemUI에서 호출
    public void ClaimReward(
        Reward reward,
        RewardItemUI item)
    {
        if (reward == null || item == null)
        {
            return;
        }

        if (!currentRewards.Contains(reward))
        {
            return;
        }

        if (GameData.instance == null ||
            GameData.instance.player == null)
        {
            Debug.LogError(
                "GameData 또는 Player 데이터가 없습니다."
            );

            return;
        }

        switch (reward.RewardType)
        {
            case RewardType.Gold:
                GameData.instance.player.Gold +=
                    reward.GoldAmount;

                RemoveReward(reward, item);
                break;

            case RewardType.Cheese:
                GameData.instance.player.Heal(
                    reward.CheeseHealAmount
                );

                RemoveReward(reward, item);
                break;

            case RewardType.Card:
                OpenCardChoice();
                break;

            case RewardType.Relic:
                // 유물 시스템 구현 후 처리
                break;
        }
    }

    private void OpenCardChoice()
    {
        if (cardChoiceOpen || cardRewardItem == null)
        {
            return;
        }

        List<CardData> availableCards =
            GetAvailableCardChoices();

        if (availableCards.Count == 0)
        {
            Debug.LogWarning(
                "선택할 카드가 없습니다."
            );

            return;
        }

        if (cardChoicePanel == null)
        {
            Debug.LogError(
                "CardChoicePanel이 연결되지 않았습니다."
            );

            return;
        }

        if (cardChoices == null ||
            cardChoices.Length == 0)
        {
            Debug.LogError(
                "CardChoicePanel에 카드 선택 UI가 없습니다."
            );

            return;
        }

        cardChoiceOpen = true;
        cardSelected = false;

        cardChoicePanel.SetActive(true);

        for (int i = 0; i < cardChoices.Length; i++)
        {
            if (cardChoices[i] == null)
            {
                continue;
            }

            if (i < availableCards.Count)
            {
                cardChoices[i].Setup(
                    availableCards[i],
                    this
                );
            }
            else
            {
                cardChoices[i].gameObject.SetActive(false);
            }
        }
    }

    private List<CardData> GetAvailableCardChoices()
    {
        List<CardData> cards = new List<CardData>();

        for (int i = 0; i < currentRewards.Count; i++)
        {
            Reward reward = currentRewards[i];

            if (reward == null ||
                reward.RewardType != RewardType.Card ||
                reward.CardData == null)
            {
                continue;
            }

            cards.Add(reward.CardData);
        }

        return cards;
    }

    // RewardCardUI에서 호출
    public void SelectCard(CardData cardData)
    {
        if (!cardChoiceOpen || cardSelected)
        {
            return;
        }

        if (cardData == null)
        {
            return;
        }

        if (!IsAvailableCard(cardData))
        {
            return;
        }

        if (!AddCardToPlayerDeck(cardData))
        {
            return;
        }

        cardSelected = true;

        // 선택지에 사용된 카드 보상을 모두 제거
        for (int i = currentRewards.Count - 1; i >= 0; i--)
        {
            if (currentRewards[i].RewardType ==
                RewardType.Card)
            {
                currentRewards.RemoveAt(i);
            }
        }

        RemoveCardRewardItem();

        cardChoiceOpen = false;

        if (cardChoicePanel != null)
        {
            cardChoicePanel.SetActive(false);
        }

        DisableCardChoices();
    }

    private bool IsAvailableCard(CardData cardData)
    {
        for (int i = 0; i < currentRewards.Count; i++)
        {
            Reward reward = currentRewards[i];

            if (reward != null &&
                reward.RewardType == RewardType.Card &&
                reward.CardData == cardData)
            {
                return true;
            }
        }

        return false;
    }

    private bool AddCardToPlayerDeck(CardData cardData)
    {
        if (GameData.instance == null ||
            GameData.instance.player == null ||
            GameData.instance.player.deck == null)
        {
            Debug.LogError(
                "플레이어 덱에 카드를 추가할 수 없습니다."
            );

            return false;
        }

        GameData.instance.player.deck.Add(
            new CardInstance(cardData)
        );

        GameData.instance.UpdatePlayerPileCount();

        return true;
    }

    private void RemoveReward(
        Reward reward,
        RewardItemUI item)
    {
        currentRewards.Remove(reward);

        spawnedItems.Remove(item);

        if (item != null)
        {
            Destroy(item.gameObject);
        }
    }

    private void RemoveCardRewardItem()
    {
        if (cardRewardItem != null)
        {
            spawnedItems.Remove(cardRewardItem);

            Destroy(cardRewardItem.gameObject);

            cardRewardItem = null;
        }
    }

    private void ClearRewardItems()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
            {
                Destroy(spawnedItems[i].gameObject);
            }
        }

        spawnedItems.Clear();
    }

    private void DisableCardChoices()
    {
        if (cardChoices == null)
        {
            return;
        }

        for (int i = 0; i < cardChoices.Length; i++)
        {
            if (cardChoices[i] == null)
            {
                continue;
            }

            Button button =
                cardChoices[i].GetComponent<Button>();

            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    private void OnContinueClicked()
    {
        Hide();

        if (SceneChanger.instance != null)
        {
            SceneChanger.instance.GoToMapScene();
        }
        else
        {
            Debug.LogError(
                "SceneChanger.instance가 없습니다."
            );
        }
    }
}
