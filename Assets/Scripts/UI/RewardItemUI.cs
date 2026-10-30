
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Image rewardIcon;

    [Header("Reward Icons")]
    [SerializeField] private Sprite goldRewardIcon;
    [SerializeField] private Sprite cardRewardIcon;

    [Header("Tooltip")]
    [SerializeField] private TooltipTrigger tooltipTrigger;
    [SerializeField] private TooltipView cheeseTooltipPrefab;
    [SerializeField] private TooltipView relicTooltipPrefab;

    private Reward reward;
    private RewardPanelUI rewardPanel;

    public void Setup(
        Reward rewardData,
        string displayName,
        RewardPanelUI panelUI)
    {
        reward = rewardData;
        rewardPanel = panelUI;

        if (rewardText != null)
        {
            rewardText.text = displayName;
        }

        UpdateRewardIcon();
        SetupTooltip();

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
            button.interactable = true;
        }
    }

    private void UpdateRewardIcon()
    {
        if (rewardIcon == null)
        {
            return;
        }

        Sprite icon = null;

        if (reward != null)
        {
            switch (reward.RewardType)
            {
                case RewardType.Gold:
                    icon = goldRewardIcon;
                    break;

                case RewardType.Cheese:
                    if (reward.CheeseData != null)
                    {
                        icon = reward.CheeseData.icon;
                    }
                    break;

                case RewardType.Card:
                    icon = cardRewardIcon;
                    break;

                case RewardType.Relic:
                    if (reward.RelicReward != null &&
                        reward.RelicReward.Relic != null)
                    {
                        icon = reward.RelicReward.Relic.Icon;
                    }
                    break;
            }
        }

        rewardIcon.sprite = icon;
        rewardIcon.enabled = icon != null;
    }

    private void SetupTooltip()
    {
        if (tooltipTrigger == null)
        {
            return;
        }

        TooltipView selectedTooltipPrefab = null;
        string title = "";
        string description = "";
        Sprite icon = null;

        if (reward == null)
        {
            tooltipTrigger.enabled = false;
            return;
        }

        switch (reward.RewardType)
        {
            case RewardType.Cheese:
                if (reward.CheeseData == null)
                {
                    tooltipTrigger.enabled = false;
                    return;
                }

                selectedTooltipPrefab = cheeseTooltipPrefab;
                title = reward.CheeseData.cheeseName;
                description = reward.CheeseData.GetDescription();
                icon = reward.CheeseData.icon;
                break;

            case RewardType.Relic:
                if (reward.RelicReward == null ||
                    reward.RelicReward.Relic == null)
                {
                    tooltipTrigger.enabled = false;
                    return;
                }

                selectedTooltipPrefab = relicTooltipPrefab;
                title = reward.RelicReward.Relic.DisplayName;
                description = reward.RelicReward.Relic.Description;
                icon = reward.RelicReward.Relic.Icon;
                break;

            default:
                tooltipTrigger.enabled = false;
                return;
        }

        tooltipTrigger.enabled = true;
        tooltipTrigger.SetTooltipPrefab(selectedTooltipPrefab);
        tooltipTrigger.SetContent(title, description, icon);
    }

    private void OnClicked()
    {
        if (rewardPanel == null || reward == null)
        {
            return;
        }

        // 카드 보상은 클릭해도 버튼 상태를 변경하지 않는다.
        if (reward.RewardType == RewardType.Card)
        {
            rewardPanel.ClaimReward(reward, this);
            return;
        }

        // 그 외 보상은 중복 클릭 방지를 위해 잠시 비활성화한다.
        if (button != null)
        {
            button.interactable = false;
        }

        rewardPanel.ClaimReward(reward, this);

        // 치즈 획득 실패 시 다시 클릭할 수 있도록 복구한다.
        if (button != null &&
            reward.RewardType == RewardType.Cheese)
        {
            button.interactable = true;
        }
    }

    private void OnDisable()
    {
        if (tooltipTrigger != null &&
            TooltipManager.Instance != null)
        {
            TooltipManager.Instance.Hide(tooltipTrigger);
        }
    }
}
