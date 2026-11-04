
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField]
    private List<EventData> eventPool =
        new List<EventData>();

    [Header("UI")]
    [SerializeField] private EventPanelUI eventPanelUI;

    private EventData currentEvent;
    private bool isChoiceSelected;

    // 실제 적용된 효과 결과
    private readonly List<string> appliedEffectSummaries =
        new List<string>();

    // =====================================================
    // Event Initialization
    // =====================================================

    private void Start()
    {
        InitializeEvent();
    }

    private void InitializeEvent()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[EventManager] GameData instance를 찾을 수 없습니다."
            );
            return;
        }

        if (eventPanelUI == null)
        {
            Debug.LogError(
                "[EventManager] EventPanelUI가 연결되지 않았습니다."
            );
            return;
        }

        currentEvent = SelectRandomEvent();

        if (currentEvent == null)
        {
            Debug.LogError(
                "[EventManager] 등장 가능한 이벤트가 없습니다."
            );
            return;
        }

        GameData.instance.MarkEventAppeared(
            currentEvent.EventId
        );

        isChoiceSelected = false;

        appliedEffectSummaries.Clear();

        eventPanelUI.Setup(currentEvent, this);

        UpdateChoiceAvailability();
    }

    // =====================================================
    // Event Selection
    // =====================================================

    private EventData SelectRandomEvent()
    {
        List<EventData> availableEvents =
            new List<EventData>();

        foreach (EventData eventData in eventPool)
        {
            if (eventData == null)
                continue;

            if (string.IsNullOrEmpty(eventData.EventId))
            {
                Debug.LogWarning(
                    "[EventManager] Event ID가 비어 있는 이벤트가 있습니다."
                );
                continue;
            }

            if (GameData.instance.HasAppearedEvent(
                eventData.EventId))
            {
                continue;
            }

            availableEvents.Add(eventData);
        }

        // 모든 이벤트가 등장했다면 기록 초기화
        if (availableEvents.Count == 0)
        {
            GameData.instance.ClearAppearedEvents();

            foreach (EventData eventData in eventPool)
            {
                if (eventData == null)
                    continue;

                if (string.IsNullOrEmpty(eventData.EventId))
                    continue;

                availableEvents.Add(eventData);
            }
        }

        if (availableEvents.Count == 0)
            return null;

        int randomIndex = Random.Range(
            0,
            availableEvents.Count
        );

        return availableEvents[randomIndex];
    }

    // =====================================================
    // Choice Conditions
    // =====================================================

    private void UpdateChoiceAvailability()
    {
        if (currentEvent == null)
            return;

        for (int i = 0; i < currentEvent.Choices.Count; i++)
        {
            bool canSelect = CheckChoiceConditions(
                currentEvent.Choices[i]
            );

            eventPanelUI.SetChoiceInteractable(
                i,
                canSelect
            );
        }
    }

    private bool CheckChoiceConditions(
        EventChoiceData choiceData)
    {
        if (choiceData == null)
            return false;

        foreach (EventConditionData condition in choiceData.Conditions)
        {
            if (!CheckCondition(condition))
                return false;
        }

        return true;
    }

    private bool CheckCondition(EventConditionData condition)
    {
        if (condition == null)
            return false;

        Entity player = GameData.instance.player;

        switch (condition.conditionType)
        {
            case EventConditionType.None:
                return true;

            case EventConditionType.MinimumHP:
                return player != null &&
                       player.CurrentHP >= condition.requiredValue;

            case EventConditionType.MinimumGold:
                return player != null &&
                       player.Gold >= condition.requiredValue;

            case EventConditionType.HasCheese:
                return HasCheese();

            case EventConditionType.HasAvailableRelic:
                return RelicManager.EnsureInstance()
                    .HasAvailableRelic();

            case EventConditionType.HasUpgradableCard:
                return HasUpgradableCard();

            default:
                return false;
        }
    }

    // =====================================================
    // Condition Helpers
    // =====================================================

    private bool HasCheese()
    {
        if (GameData.instance == null)
            return false;

        foreach (CheeseData cheese in GameData.instance.CheeseInventory)
        {
            if (cheese != null)
                return true;
        }

        return false;
    }

    private bool HasUpgradableCard()
    {
        Entity player = GameData.instance.player;

        if (player == null || player.deck == null)
            return false;

        foreach (CardInstance card in player.deck)
        {
            if (card != null &&
                card.data != null &&
                !card.upgraded)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================================
    // Choice Selection
    // =====================================================

    public void SelectChoice(int index)
    {
        if (isChoiceSelected)
            return;

        if (currentEvent == null)
            return;

        if (index < 0 ||
            index >= currentEvent.Choices.Count)
        {
            return;
        }

        EventChoiceData choiceData =
            currentEvent.Choices[index];

        // 선택 순간 조건 재검사
        if (!CheckChoiceConditions(choiceData))
        {
            UpdateChoiceAvailability();
            return;
        }

        isChoiceSelected = true;

        appliedEffectSummaries.Clear();

        ApplyChoiceEffects(choiceData);

        eventPanelUI.ShowResult(
            BuildResultDescription(choiceData)
        );
    }

    // =====================================================
    // Result Description
    // =====================================================

    private string BuildResultDescription(
    EventChoiceData choiceData)
    {
        StringBuilder result = new StringBuilder();

        result.Append(choiceData.ResultDescription);

        if (appliedEffectSummaries.Count > 0)
        {
            result.Append("\n(");

            result.Append(
                string.Join(", ", appliedEffectSummaries)
            );

            result.Append(")");
        }

        return result.ToString();
    }

    // =====================================================
    // Choice Effects
    // =====================================================

    private void ApplyChoiceEffects(
        EventChoiceData choiceData)
    {
        foreach (EventEffectData effect in choiceData.Effects)
        {
            if (effect == null)
                continue;

            ApplyEffect(effect);
        }
    }

    private void ApplyEffect(EventEffectData effect)
    {
        Entity player = GameData.instance.player;

        switch (effect.effectType)
        {
            case EventEffectType.None:
                break;

            // -----------------------------------------
            // Gold
            // -----------------------------------------

            case EventEffectType.GainGold:
                {
                    int previousGold = player.Gold;

                    player.Gold += effect.amount;

                    int actualAmount =
                        player.Gold - previousGold;

                    if (actualAmount > 0)
                    {
                        appliedEffectSummaries.Add(
                            $"+{actualAmount} 골드"
                        );
                    }

                    break;
                }

            case EventEffectType.LoseGold:
                {
                    int previousGold = player.Gold;

                    player.Gold -= effect.amount;

                    int actualAmount =
                        previousGold - player.Gold;

                    if (actualAmount > 0)
                    {
                        appliedEffectSummaries.Add(
                            $"-{actualAmount} 골드"
                        );
                    }

                    break;
                }

            // -----------------------------------------
            // HP
            // -----------------------------------------

            case EventEffectType.GainHP:
                {
                    int previousHP = player.CurrentHP;

                    player.Heal(effect.amount);

                    int actualAmount =
                        player.CurrentHP - previousHP;

                    if (actualAmount > 0)
                    {
                        appliedEffectSummaries.Add(
                            $"+{actualAmount} 체력"
                        );
                    }

                    break;
                }

            case EventEffectType.LoseHP:
                {
                    int previousHP = player.CurrentHP;

                    player.CurrentHP = Mathf.Max(
                        1,
                        player.CurrentHP - effect.amount
                    );

                    int actualAmount =
                        previousHP - player.CurrentHP;

                    if (actualAmount > 0)
                    {
                        appliedEffectSummaries.Add(
                            $"-{actualAmount} 체력"
                        );
                    }

                    break;
                }

            case EventEffectType.GainMaxHP:
                {
                    int previousMaxHP = player.MaxHP;

                    player.MaxHP += effect.amount;

                    int actualAmount =
                        player.MaxHP - previousMaxHP;

                    if (actualAmount > 0)
                    {
                        appliedEffectSummaries.Add(
                            $"+{actualAmount} 최대 체력"
                        );
                    }

                    break;
                }

            // -----------------------------------------
            // Relic
            // -----------------------------------------

            case EventEffectType.GainRandomRelic:
                GainRandomRelic();
                break;

            // -----------------------------------------
            // Cheese
            // -----------------------------------------

            case EventEffectType.LoseRandomCheese:
                {
                    if (GameData.instance.RemoveRandomCheese())
                    {
                        appliedEffectSummaries.Add(
                            "치즈 1개 소모"
                        );
                    }
                    else
                    {
                        appliedEffectSummaries.Add(
                            "소모할 치즈 없음"
                        );
                    }

                    break;
                }

            case EventEffectType.GainRandomCheese:
                GainRandomCheese();
                break;

            // -----------------------------------------
            // Card Upgrade
            // -----------------------------------------

            case EventEffectType.UpgradeRandomCard:
                UpgradeRandomCard();
                break;
        }
    }

    // =====================================================
    // Random Card Upgrade
    // =====================================================

    private void UpgradeRandomCard()
    {
        Entity player = GameData.instance.player;

        if (player == null || player.deck == null)
        {
            appliedEffectSummaries.Add(
                "강화 가능한 카드 없음"
            );
            return;
        }

        List<CardInstance> availableCards =
            new List<CardInstance>();

        foreach (CardInstance card in player.deck)
        {
            if (card == null ||
                card.data == null ||
                card.upgraded)
            {
                continue;
            }

            availableCards.Add(card);
        }

        if (availableCards.Count == 0)
        {
            Debug.LogWarning(
                "[EventManager] 강화할 수 있는 카드가 없습니다."
            );

            appliedEffectSummaries.Add(
                "강화 가능한 카드 없음"
            );
            return;
        }

        int randomIndex = Random.Range(
            0,
            availableCards.Count
        );

        CardInstance selectedCard =
            availableCards[randomIndex];

        if (selectedCard.Upgrade())
        {
            Debug.Log(
                $"[EventManager] 카드 강화 완료: " +
                $"{selectedCard.GetDisplayName()}"
            );

            appliedEffectSummaries.Add(
                $"{selectedCard.data.cardName} 강화"
            );
        }
        else
        {
            appliedEffectSummaries.Add(
                "카드 강화 실패"
            );
        }
    }

    // =====================================================
    // Random Relic
    // =====================================================

    private void GainRandomRelic()
    {
        RelicManager relicManager =
            RelicManager.EnsureInstance();

        List<RelicData> relics =
            relicManager.DrawRandomAvailableRelics(1);

        if (relics == null || relics.Count == 0)
        {
            Debug.LogWarning(
                "[EventManager] 획득 가능한 유물이 없습니다."
            );

            appliedEffectSummaries.Add(
                "획득 가능한 유물 없음"
            );
            return;
        }

        RelicData selectedRelic = relics[0];

        relicManager.Acquire(
            selectedRelic,
            RelicSource.Event
        );

        appliedEffectSummaries.Add(
            $"{selectedRelic.DisplayName} 획득"
        );
    }

    // =====================================================
    // Random Cheese
    // =====================================================

    private void GainRandomCheese()
    {
        CheeseData[] cheesePool =
            GameData.instance.CheesePool;

        if (cheesePool == null || cheesePool.Length == 0)
        {
            appliedEffectSummaries.Add(
                "획득 가능한 치즈 없음"
            );
            return;
        }

        List<CheeseData> availableCheeses =
            new List<CheeseData>();

        foreach (CheeseData cheese in cheesePool)
        {
            if (cheese != null)
                availableCheeses.Add(cheese);
        }

        if (availableCheeses.Count == 0)
        {
            appliedEffectSummaries.Add(
                "획득 가능한 치즈 없음"
            );
            return;
        }

        CheeseData randomCheese =
            availableCheeses[
                Random.Range(0, availableCheeses.Count)
            ];

        if (GameData.instance.AddCheese(randomCheese))
        {
            appliedEffectSummaries.Add(
                $"{randomCheese.cheeseName} 획득"
            );
        }
        else
        {
            Debug.LogWarning(
                "[EventManager] 치즈 인벤토리가 가득 찼습니다."
            );

            appliedEffectSummaries.Add(
                "치즈 인벤토리 가득 참"
            );
        }
    }

    // =====================================================
    // Scene Transition
    // =====================================================

    public void ContinueToMap()
    {
        if (!isChoiceSelected)
            return;

        if (SceneChanger.instance == null)
        {
            Debug.LogError(
                "[EventManager] SceneChanger를 찾을 수 없습니다."
            );
            return;
        }

        SceneChanger.instance.GoToMapScene();
    }
}
