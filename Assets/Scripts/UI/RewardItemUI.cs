
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text rewardText;

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
        if (rewardPanel == null || reward == null)
        {
            return;
        }

        if (button != null)
        {
            button.interactable = false;
        }

        rewardPanel.ClaimReward(reward, this);

        // 카드 보상은 선택 패널을 열어야 하므로
        // 여기서 버튼을 비활성화하지 않는다.
        if (reward != null &&
            reward.RewardType == RewardType.Card &&
            button != null)
        {
            button.interactable = true;
        }
    }
}
