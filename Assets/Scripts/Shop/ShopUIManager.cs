using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 씬의 UI 총괄
/// 슬롯 생성, 클릭 처리, 골드 표시, 나가기 버튼을 관리
/// </summary>
public class ShopUIManager : MonoBehaviour
{
    // =====================================================
    // 설정값
    // =====================================================

    private const float MESSAGE_DURATION = 1.5f; // 안내 메시지 표시 시간
    private const int CARD_GRID_COUNT = 4; // CardSlotParent(2x2 그리드)에 들어가는 카드 수, 나머지는 LastRowParent로

    // =====================================================
    // UI 참조
    // =====================================================

    [Header("Slots")]
    [SerializeField] private ShopEntryUI entryPrefab;
    [SerializeField] private ShopEntryUI relicEntryPrefab;
    [SerializeField] private Transform cardSlotParent;  // 카드 앞 4장 (2x2 그리드)
    [SerializeField] private Transform lastRowParent;    // 카드 5번째 + RelicGroup (한 줄, 가운데 정렬)
    [SerializeField] private Transform relicGroupParent; // 유물 전체가 들어가는 중첩 그룹 (LastRowParent의 자식)

    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;

    [Header("Buttons")]
    [SerializeField] private Button exitButton;

    [Header("Message")]
    [SerializeField] private TMP_Text messageText;

    private readonly List<ShopEntryUI> slots = new List<ShopEntryUI>();

    private Entity subscribedPlayer;

    private Coroutine messageRoutine;

    // =====================================================
    // 초기화
    // =====================================================

    private void Start()
    {
        if (ShopManager.instance == null)
        {
            Debug.LogWarning("[ShopUI] ShopManager가 없어 UI를 생성하지 않음");
            return;
        }

        if (GameData.instance == null)
        {
            Debug.LogWarning("[ShopUI] GameData가 없어 UI를 생성하지 않음 (단독 테스트라면 ShopTest를 활성화)");
            return;
        }

        BuildSlots();

        // 골드 변경 이벤트 등록
        subscribedPlayer = GameData.instance.player;
        subscribedPlayer.OnGoldChanged += OnGoldChanged;

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        // 초기 표시
        OnGoldChanged(subscribedPlayer.Gold);
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (subscribedPlayer != null)
        {
            subscribedPlayer.OnGoldChanged -= OnGoldChanged;
        }
    }

    // =====================================================
    // 슬롯 생성
    // =====================================================

    /// <summary>
    /// ShopManager의 진열 목록으로 카드 슬롯, 유물 슬롯 생성
    /// </summary>
    private void BuildSlots()
    {
        ClearSlots();

        List<ShopEntry> cardEntries =
            ShopManager.instance.CardEntries;

        if (cardEntries != null)
        {
            for (int i = 0; i < cardEntries.Count; i++)
            {
                // 앞 CARD_GRID_COUNT장은 그리드, 나머지(5번째)는 마지막 줄로
                Transform parent =
                    i < CARD_GRID_COUNT
                    ? cardSlotParent
                    : lastRowParent;

                CreateSlot(cardEntries[i], parent);
            }
        }

        List<ShopEntry> relicEntries =
            ShopManager.instance.RelicEntries;

        if (relicEntries != null)
        {
            Transform relicParent =
                relicGroupParent != null
                ? relicGroupParent
                : lastRowParent;

            foreach (ShopEntry entry in relicEntries)
            {
                CreateSlot(entry, relicParent);
            }
        }
    }

    private void CreateSlot(ShopEntry entry, Transform parent)
    {
        ShopEntryUI prefab =
            entry.relic != null
            ? relicEntryPrefab
            : entryPrefab;

        ShopEntryUI slot =
            Instantiate(prefab, parent);

        slot.Setup(entry, this);

        slots.Add(slot);
    }

    private void ClearSlots()
    {
        foreach (ShopEntryUI slot in slots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }

        slots.Clear();
    }

    // =====================================================
    // 클릭 처리
    // =====================================================

    /// <summary>
    /// 슬롯 클릭 시 구매를 시도하고 결과에 따라 UI 갱신
    /// </summary>
    public void OnEntryClicked(ShopEntryUI slot)
    {
        ShopEntry entry = slot.Entry;

        bool success =
            ShopManager.instance.TryPurchase(entry);

        if (!success)
        {
            ShowMessage(
                entry.purchased
                ? "이미 구매한 상품입니다."
                : "골드가 부족합니다."
            );

            return;
        }

        RefreshAll(GameData.instance.player.Gold);
    }

    private void OnExitClicked()
    {
        ShopManager.instance.ExitShop();
    }

    // =====================================================
    // 골드 / 슬롯 갱신
    // =====================================================

    private void OnGoldChanged(int gold)
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }

        RefreshAll(gold);
    }

    private void RefreshAll(int gold)
    {
        foreach (ShopEntryUI slot in slots)
        {
            slot.Refresh(gold);
        }
    }

    // =====================================================
    // 안내 메시지
    // =====================================================

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    private IEnumerator MessageRoutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(MESSAGE_DURATION);

        messageText.gameObject.SetActive(false);
    }
}
