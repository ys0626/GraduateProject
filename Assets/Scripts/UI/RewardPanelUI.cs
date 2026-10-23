
using System.Collections.Generic;
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
    [SerializeField] private CardSelectionUI[] cardChoices;
    [SerializeField] private Button cardSkipButton;

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

        if (cardSkipButton != null)
        {
            cardSkipButton.onClick.RemoveListener(
                OnCardSkipClicked
            );

            cardSkipButton.onClick.AddListener(
                OnCardSkipClicked
            );
        }

        Hide();
    }

    // =====================================================
    // 보상 화면 표시
    // =====================================================

    public void ShowRewards()
    {
        if (RewardManager.Instance == null)
        {
            Debug.LogError(
                "[RewardPanelUI] RewardManager가 없습니다."
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

    // =====================================================
    // 보상 항목 표시
    // =====================================================

    private void DisplayRewards()
    {
        if (rewardContent == null ||
            rewardItemPrefab == null)
        {
            Debug.LogError(
                "[RewardPanelUI] Reward Content 또는 " +
                "Reward Item Prefab이 연결되지 않았습니다."
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
                    if (reward.CheeseData != null)
                    {
                        CreateRewardItem(
                            reward,
                            reward.CheeseData.cheeseName
                        );
                    }
                    break;

                case RewardType.Card:
                    // 카드가 여러 장이어도 보상 항목은 하나만 표시
                    if (!cardRewardDisplayed)
                    {
                        cardRewardItem = CreateRewardItem(
                            reward,
                            "덱에 추가할 카드를 선택하세요!"
                        );

                        cardRewardDisplayed = true;
                    }
                    break;

                case RewardType.Relic:
                    if (reward.RelicReward != null &&
                        reward.RelicReward.Relic != null)
                    {
                        CreateRewardItem(
                            reward,
                            reward.RelicReward.Relic.DisplayName
                        );
                    }
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

    // =====================================================
    // 보상 획득
    // RewardItemUI에서 호출
    // =====================================================

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

        if (GameData.instance == null)
        {
            Debug.LogError(
                "[RewardPanelUI] GameData가 없습니다."
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
                ClaimCheeseReward(reward, item);
                break;

            case RewardType.Card:
                OpenCardChoice();
                break;

            case RewardType.Relic:
                ClaimRelicReward(reward, item);
                break;
        }
    }

    private void ClaimCheeseReward(
        Reward reward,
        RewardItemUI item)
    {
        if (reward.CheeseData == null)
        {
            Debug.LogWarning(
                "[RewardPanelUI] 치즈 데이터가 없습니다."
            );

            return;
        }

        bool added = GameData.instance.AddCheese(
            reward.CheeseData
        );

        if (added)
        {
            if (PersistentUI.Instance != null)
            {
                PersistentUI.Instance.RefreshCheeseSlots();
            }

            RemoveReward(reward, item);
        }
        else
        {
            if (PersistentUI.Instance != null)
            {
                PersistentUI.Instance
                    .ShowCheeseInventoryFullPopup();
            }
        }
    }

    // 유물 보상 획득
    private void ClaimRelicReward(
        Reward reward,
        RewardItemUI item)
    {
        if (reward.RelicReward == null ||
            reward.RelicReward.Relic == null)
        {
            Debug.LogWarning(
                "[RewardPanelUI] 유물 보상 데이터가 없습니다."
            );

            return;
        }

        RelicManager relicManager =
            RelicManager.EnsureInstance();

        RelicInstance acquiredRelic =
            relicManager.Acquire(
                reward.RelicReward.Relic,
                reward.RelicReward.Source
            );

        // 실제 획득에 성공했을 때만 보상 항목 제거
        if (acquiredRelic != null)
        {
            RemoveReward(reward, item);
        }
    }

    // =====================================================
    // 카드 선택 패널 열기
    // =====================================================

    private void OpenCardChoice()
    {
        if (cardChoiceOpen || cardSelected)
        {
            return;
        }

        if (cardChoicePanel == null)
        {
            Debug.LogError(
                "[RewardPanelUI] Card Choice Panel이 없습니다."
            );

            return;
        }

        if (cardChoices == null ||
            cardChoices.Length == 0)
        {
            Debug.LogError(
                "[RewardPanelUI] Card Choices가 비어 있습니다."
            );

            return;
        }

        List<CardData> availableCards =
            GetAvailableCardChoices();

        if (availableCards.Count == 0)
        {
            Debug.LogWarning(
                "[RewardPanelUI] 선택할 카드가 없습니다."
            );

            return;
        }

        cardChoiceOpen = true;

        cardChoicePanel.SetActive(true);

        for (int i = 0; i < cardChoices.Length; i++)
        {
            CardSelectionUI choice = cardChoices[i];

            if (choice == null)
            {
                Debug.LogWarning(
                    "[RewardPanelUI] Card Choices의 " +
                    i + "번째 슬롯이 비어 있습니다."
                );

                continue;
            }

            if (i < availableCards.Count)
            {
                choice.gameObject.SetActive(true);

                choice.Setup(
                    availableCards[i],
                    SelectCard
                );
            }
            else
            {
                choice.gameObject.SetActive(false);
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

    // =====================================================
    // 카드 선택
    // CardSelectionUI에서 호출
    // =====================================================

    public void SelectCard(CardData cardData)
    {
        if (!cardChoiceOpen || cardSelected)
        {
            return;
        }

        if (cardData == null ||
            !IsAvailableCard(cardData))
        {
            return;
        }

        if (PlayerDeckManager.instance == null)
        {
            Debug.LogError(
                "[RewardPanelUI] PlayerDeckManager가 없습니다."
            );

            return;
        }

        // 실제 덱에는 선택한 카드 한 장만 추가
        PlayerDeckManager.instance.AddCardToPlayerDeck(
            cardData
        );

        cardSelected = true;

        CompleteCardReward();
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

    // =====================================================
    // 카드 선택 완료 또는 넘기기
    // =====================================================

    private void OnCardSkipClicked()
    {
        if (!cardChoiceOpen || cardSelected)
        {
            return;
        }

        // 카드를 획득하지 않고 선택 창만 닫는다.
        // 카드 보상 버튼과 보상 데이터는 유지한다.
        cardChoiceOpen = false;

        if (cardChoicePanel != null)
        {
            cardChoicePanel.SetActive(false);
        }
    }


    private void CompleteCardReward()
    {
        // 카드 보상 선택지 전체 제거
        for (int i = currentRewards.Count - 1; i >= 0; i--)
        {
            Reward reward = currentRewards[i];

            if (reward != null &&
                reward.RewardType == RewardType.Card)
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
    }

    // =====================================================
    // 보상 항목 제거
    // =====================================================

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
        if (cardRewardItem == null)
        {
            return;
        }

        spawnedItems.Remove(cardRewardItem);

        Destroy(cardRewardItem.gameObject);

        cardRewardItem = null;
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
        cardRewardItem = null;
    }

    // =====================================================
    // 보상 화면 넘기기
    // =====================================================

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
                "[RewardPanelUI] SceneChanger가 없습니다."
            );
        }
    }
}
