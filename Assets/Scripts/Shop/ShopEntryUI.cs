using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 슬롯 1개(카드 또는 유물)의 표시와 클릭 감지
/// 구매 판단은 하지 않고, 클릭을 ShopUIManager에 전달
/// </summary>
public class ShopEntryUI : MonoBehaviour
{
    // =====================================================
    // UI 참조 (프리팹에서 연결)
    // =====================================================

    [Header("Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Price")]
    [SerializeField] private TMP_Text priceText;

    [Header("Sold Out")]
    [SerializeField] private GameObject soldOutOverlay;

    [Header("Price Colors")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.red;

    [System.NonSerialized] private ShopEntry entry;
    [System.NonSerialized] private ShopUIManager owner;

    private Button button;

    public ShopEntry Entry => entry;

    // =====================================================
    // 초기화
    // =====================================================

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
    }

    // =====================================================
    // 표시 설정
    // =====================================================

    /// <summary>
    /// 진열 항목의 정보를 슬롯에 표시
    /// </summary>
    public void Setup(ShopEntry shopEntry, ShopUIManager uiManager)
    {
        entry = shopEntry;
        owner = uiManager;

        if (entry.card != null)
        {
            SetupCard(entry.card);
        }
        else if (entry.relic != null)
        {
            SetupRelic(entry.relic);
        }

        priceText.text = entry.price.ToString();
    }

    /// <summary>
    /// 카드 슬롯 표시 (이름, 코스트, 설명, 아트워크)
    /// </summary>
    private void SetupCard(CardData card)
    {
        iconImage.sprite = card.artwork;

        nameText.text = card.cardName;
        descriptionText.text = card.description;

        if (costText != null)
        {
            costText.gameObject.SetActive(true);
            costText.text = card.cost.ToString();
        }
    }

    /// <summary>
    /// 유물 슬롯 표시 (유물은 코스트가 없으므로 코스트 텍스트를 숨김)
    /// </summary>
    private void SetupRelic(RelicData relic)
    {
        iconImage.sprite = relic.Icon;

        nameText.text = relic.DisplayName;
        descriptionText.text = relic.Description;

        if (costText != null)
        {
            costText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // 상태 갱신
    // =====================================================

    /// <summary>
    /// 구매 여부, 골드 충분 여부에 따라 슬롯 상태 갱신
    /// </summary>
    public void Refresh(int currentGold)
    {
        if (entry == null)
            return;

        bool soldOut = entry.purchased;

        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(soldOut);
        }

        if (button != null)
        {
            button.interactable = !soldOut;
        }

        priceText.color =
            currentGold >= entry.price
            ? affordableColor
            : unaffordableColor;
    }

    // =====================================================
    // 클릭
    // =====================================================

    private void OnClicked()
    {
        if (entry == null || owner == null)
            return;

        owner.OnEntryClicked(this);
    }
}
