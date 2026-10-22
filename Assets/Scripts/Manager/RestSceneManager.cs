using UnityEngine;
using UnityEngine.UI;

public class RestSceneManager : MonoBehaviour
{
    [Header("Rest Settings")]
    [SerializeField, Range(0f, 100f)]
    private float healPercentage = 30f;

    [Header("Panels")]
    [SerializeField] private GameObject choicePanel;

    [Header("Buttons")]
    [SerializeField] private Button restButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button continueButton;

    private bool actionCompleted;

    private void OnEnable()
    {
        if (restButton != null)
        {
            restButton.onClick.AddListener(Rest);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OpenUpgradePopup);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToMap);
        }

        if (CardPopupManager.instance != null)
        {
            CardPopupManager.instance.OnUpgradeCompleted +=
                HandleUpgradeCompleted;
        }
    }

    private void Start()
    {
        actionCompleted = false;

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        if (restButton != null)
        {
            restButton.interactable = true;
        }

        UpdateUpgradeButton();

        if (CardPopupManager.instance == null)
        {
            Debug.LogError(
                "[RestSceneManager] CardPopupManager가 없습니다.",
                this
            );
        }
    }

    private void OnDisable()
    {
        if (restButton != null)
        {
            restButton.onClick.RemoveListener(Rest);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OpenUpgradePopup);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ContinueToMap);
        }

        if (CardPopupManager.instance != null)
        {
            CardPopupManager.instance.OnUpgradeCompleted -=
                HandleUpgradeCompleted;
        }
    }

    // =====================================================
    // 강화 가능한 카드 확인
    // =====================================================

    private void UpdateUpgradeButton()
    {
        if (upgradeButton == null)
            return;

        Entity player = null;

        if (GameData.instance != null)
        {
            player = GameData.instance.player;
        }

        bool hasUpgradableCard = false;

        if (player != null && player.deck != null)
        {
            foreach (CardInstance card in player.deck)
            {
                if (card != null &&
                    card.data != null &&
                    !card.upgraded)
                {
                    hasUpgradableCard = true;
                    break;
                }
            }
        }

        upgradeButton.interactable =
            hasUpgradableCard && !actionCompleted;
    }

    // =====================================================
    // 휴식하기
    // =====================================================

    private void Rest()
    {
        if (actionCompleted)
            return;

        if (GameData.instance == null ||
            GameData.instance.player == null)
        {
            Debug.LogError(
                "[RestSceneManager] 플레이어 데이터가 없습니다.",
                this
            );

            return;
        }

        Entity player = GameData.instance.player;

        int healAmount = Mathf.CeilToInt(
            player.MaxHP * healPercentage / 100f
        );

        player.Heal(healAmount);

        CompleteAction();
    }

    // =====================================================
    // 강화하기
    // =====================================================

    private void OpenUpgradePopup()
    {
        if (actionCompleted)
            return;

        if (CardPopupManager.instance == null)
        {
            Debug.LogError(
                "[RestSceneManager] CardPopupManager가 없습니다.",
                this
            );

            return;
        }

        Entity player = GameData.instance != null
            ? GameData.instance.player
            : null;

        if (player == null)
        {
            Debug.LogError(
                "[RestSceneManager] 플레이어 데이터가 없습니다.",
                this
            );

            return;
        }

        // 클릭 시점에도 강화 가능한 카드가 있는지 재확인한다.
        bool hasUpgradableCard = false;

        foreach (CardInstance card in player.deck)
        {
            if (card != null &&
                card.data != null &&
                !card.upgraded)
            {
                hasUpgradableCard = true;
                break;
            }
        }

        if (!hasUpgradableCard)
        {
            UpdateUpgradeButton();
            return;
        }

        CardPopupManager.instance.OpenUpgradableDeck();
    }

    private void HandleUpgradeCompleted(CardInstance upgradedCard)
    {
        if (upgradedCard == null)
            return;

        CompleteAction();
    }

    // =====================================================
    // 행동 완료
    // =====================================================

    private void CompleteAction()
    {
        if (actionCompleted)
            return;

        actionCompleted = true;

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        if (restButton != null)
        {
            restButton.interactable = false;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
        }
    }

    // =====================================================
    // 맵으로 돌아가기
    // =====================================================

    private void ContinueToMap()
    {
        if (!actionCompleted)
            return;

        if (SceneChanger.instance != null)
        {
            SceneChanger.instance.GoToMapScene();
        }
        else
        {
            Debug.LogError(
                "[RestSceneManager] SceneChanger가 없습니다.",
                this
            );
        }
    }
}
