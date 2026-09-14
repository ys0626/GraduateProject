using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 씬의 진열, 구매, 퇴장 처리
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    // =====================================================
    // 카드/유물 풀 (Inspector에서 등록)
    // =====================================================

    [Header("카드/유물 풀")]
    [SerializeField] private List<CardData> cardPool;
    [SerializeField] private List<RelicData> relicPool;

    // =====================================================
    // 진열 상태
    // =====================================================

    public List<ShopEntry> CardEntries { get; private set; }

    public ShopEntry RelicEntry { get; private set; }

    // =====================================================
    // 초기화
    // =====================================================

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        GenerateShop();
    }

    // =====================================================
    // 상점 진열
    // =====================================================

    /// <summary>
    /// 카드 5개, 유물 1개를 랜덤으로 뽑아 진열
    /// </summary>
    private void GenerateShop()
    {
        CardEntries =
            ShopGenerator.GenerateCardEntries(cardPool);

        RelicEntry =
            ShopGenerator.GenerateRelicEntry(relicPool);
    }

    // =====================================================
    // 구매 처리
    // =====================================================

    /// <summary>
    /// 아이템 구매 시도. 골드가 충분하면 차감 후 적용
    /// </summary>
    public bool TryPurchase(ShopEntry entry)
    {
        if (entry == null || entry.purchased)
        {
            return false;
        }

        Entity player =
            GameData.instance.player;

        if (player.Gold < entry.price)
        {
            return false;
        }

        player.Gold -= entry.price;

        entry.purchased = true;

        if (entry.card != null)
        {
            ApplyCardPurchase(entry.card);
        }
        else if (entry.relic != null)
        {
            ApplyRelicPurchase(entry.relic);
        }

        return true;
    }

    // =====================================================
    // 구매 적용
    // =====================================================

    /// <summary>
    /// 구매한 카드를 플레이어 덱에 추가
    /// </summary>
    private void ApplyCardPurchase(CardData card)
    {
        Entity player =
            GameData.instance.player;

        player.deck.Add(new CardInstance(card));

        player.DeckCount = player.deck.Count;
    }

    /// <summary>
    /// 구매한 유물을 인벤토리에 추가 (유물 파트 연동 지점)
    /// </summary>
    private void ApplyRelicPurchase(RelicData relic)
    {
        // TODO: 유물 인벤토리 시스템 완성되면 연동
        // 예: RelicInventory.instance.AddRelic(relic);
    }

    // =====================================================
    // 상점 퇴장
    // =====================================================

    /// <summary>
    /// 상점을 나가고 맵으로 복귀
    /// </summary>
    public void ExitShop()
    {
        // TODO: 맵 파트 완성되면 연동
        // 예: SceneManager.LoadScene("Map");
    }
}
