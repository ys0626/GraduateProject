using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Serializable]
    private class PriceRange
    {
        [Min(0)] public int minPrice = 10;
        [Min(0)] public int maxPrice = 30;

        public int GetRandomPrice()
        {
            int min = Mathf.Max(0, minPrice);
            int max = Mathf.Max(min, maxPrice);

            return UnityEngine.Random.Range(min, max + 1);
        }
    }

    [Header("Card Slots")]
    [SerializeField]
    private ShopCardSlotUI[] commonCardSlots =
        new ShopCardSlotUI[3];

    [SerializeField]
    private ShopCardSlotUI rareCardSlot;

    [Header("Other Item Rows")]
    [SerializeField]
    private Transform relicContainer;

    [SerializeField]
    private Transform cheeseContainer;

    [SerializeField]
    private ShopItemUI shopItemPrefab;

    [Header("Price Ranges")]
    [SerializeField]
    private PriceRange commonCardPrice =
        new PriceRange { minPrice = 20, maxPrice = 35 };

    [SerializeField]
    private PriceRange rareCardPrice =
        new PriceRange { minPrice = 50, maxPrice = 80 };

    [SerializeField]
    private PriceRange relicPrice =
        new PriceRange { minPrice = 60, maxPrice = 100 };

    [SerializeField]
    private PriceRange cheesePrice =
        new PriceRange { minPrice = 15, maxPrice = 30 };

    [Header("Shop UI")]
    [SerializeField]
    private Button leaveShopButton;

    private readonly List<ShopItemUI> relicItems =
        new List<ShopItemUI>();

    private readonly List<ShopItemUI> cheeseItems =
        new List<ShopItemUI>();

    private void Awake()
    {
        if (leaveShopButton != null)
        {
            leaveShopButton.onClick.AddListener(LeaveShop);
        }
    }

    private void Start()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[ShopManager] GameData가 없습니다.",
                this
            );

            return;
        }

        GenerateShop();
    }

    private void OnDestroy()
    {
        if (leaveShopButton != null)
        {
            leaveShopButton.onClick.RemoveListener(LeaveShop);
        }
    }

    // =====================================================
    // 상품 생성
    // =====================================================

    public void GenerateShop()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[ShopManager] GameData가 없습니다.",
                this
            );

            return;
        }

        GenerateCommonCards();
        GenerateRareCard();
        GenerateRelics();
        GenerateCheeses();
    }

    // =====================================================
    // 일반 카드 3개
    // =====================================================

    private void GenerateCommonCards()
    {
        if (commonCardSlots == null)
        {
            return;
        }

        List<CardData> cards = DrawUniqueCards(
            GameData.instance.CommonCardPool,
            commonCardSlots.Length
        );

        for (int i = 0; i < commonCardSlots.Length; i++)
        {
            ShopCardSlotUI slot = commonCardSlots[i];

            if (slot == null)
            {
                continue;
            }

            if (i >= cards.Count)
            {
                Debug.LogWarning(
                    "[ShopManager] 일반 카드 풀이 부족합니다."
                );

                continue;
            }

            CardData card = cards[i];
            int price = commonCardPrice.GetRandomPrice();

            slot.Setup(
                card,
                price,
                selectedCard =>
                    TryBuyCard(slot, selectedCard, price)
            );
        }
    }

    // =====================================================
    // 희귀 카드 1개
    // =====================================================

    private void GenerateRareCard()
    {
        if (rareCardSlot == null)
        {
            return;
        }

        List<CardData> cards = DrawUniqueCards(
            GameData.instance.RareCardPool,
            1
        );

        if (cards.Count == 0)
        {
            Debug.LogWarning(
                "[ShopManager] 희귀 카드 풀이 비어 있습니다."
            );

            return;
        }

        CardData card = cards[0];
        int price = rareCardPrice.GetRandomPrice();

        rareCardSlot.Setup(
            card,
            price,
            selectedCard =>
                TryBuyCard(rareCardSlot, selectedCard, price)
        );
    }

    // =====================================================
    // 중복 없는 카드 추첨
    // =====================================================

    private List<CardData> DrawUniqueCards(
        CardData[] pool,
        int count)
    {
        List<CardData> candidates = new List<CardData>();
        List<CardData> results = new List<CardData>();

        if (pool == null)
        {
            return results;
        }

        foreach (CardData card in pool)
        {
            if (card != null && !candidates.Contains(card))
            {
                candidates.Add(card);
            }
        }

        int drawCount = Mathf.Min(count, candidates.Count);

        for (int i = 0; i < drawCount; i++)
        {
            int index = UnityEngine.Random.Range(
                0,
                candidates.Count
            );

            results.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        return results;
    }

    // =====================================================
    // 유물 상품 2개
    // =====================================================

    private void GenerateRelics()
    {
        if (relicContainer == null || shopItemPrefab == null)
        {
            Debug.LogError(
                "[ShopManager] 유물 컨테이너 또는 프리팹이 없습니다.",
                this
            );

            return;
        }

        ClearItemList(relicItems);

        RelicManager manager = RelicManager.EnsureInstance();

        if (manager == null)
        {
            Debug.LogError(
                "[ShopManager] RelicManager를 생성하거나 찾을 수 없습니다.",
                this
            );

            return;
        }

        List<RelicData> relics =
            manager.DrawRandomAvailableRelics(2);

        foreach (RelicData relic in relics)
        {
            if (relic == null)
            {
                continue;
            }

            int price = relicPrice.GetRandomPrice();

            ShopItemUI item = Instantiate(
                shopItemPrefab,
                relicContainer
            );

            item.Setup(
                relic.Icon,
                relic.DisplayName,
                relic.Description,
                price,
                () => TryBuyRelic(item, relic, price)
            );

            relicItems.Add(item);
        }

        if (relics.Count < 2)
        {
            Debug.Log(
                $"[ShopManager] 등장 가능한 유물이 {relics.Count}개뿐입니다."
            );
        }
    }

    // =====================================================
    // 치즈 상품 2개
    // =====================================================

    private void GenerateCheeses()
    {
        if (cheeseContainer == null || shopItemPrefab == null)
        {
            Debug.LogError(
                "[ShopManager] 치즈 컨테이너 또는 프리팹이 없습니다.",
                this
            );

            return;
        }

        ClearItemList(cheeseItems);

        CheeseData[] pool = GameData.instance.CheesePool;

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning(
                "[ShopManager] 치즈 풀이 비어 있습니다."
            );

            return;
        }

        List<CheeseData> candidates = new List<CheeseData>();

        foreach (CheeseData cheese in pool)
        {
            if (cheese != null)
            {
                candidates.Add(cheese);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning(
                "[ShopManager] 사용할 수 있는 치즈가 없습니다."
            );

            return;
        }

        // 치즈는 같은 종류가 중복으로 등장할 수 있다.
        for (int i = 0; i < 2; i++)
        {
            CheeseData cheese = candidates[
                UnityEngine.Random.Range(0, candidates.Count)
            ];

            int price = cheesePrice.GetRandomPrice();

            ShopItemUI item = Instantiate(
                shopItemPrefab,
                cheeseContainer
            );

            item.Setup(
                cheese.icon,
                cheese.cheeseName,
                cheese.GetDescription(),
                price,
                () => TryBuyCheese(item, cheese, price)
            );

            cheeseItems.Add(item);
        }
    }

    // =====================================================
    // 기존 상품 오브젝트 정리
    // =====================================================

    private void ClearItemList(List<ShopItemUI> items)
    {
        foreach (ShopItemUI item in items)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        items.Clear();
    }

    // =====================================================
    // 구매 공통 검사
    // =====================================================

    private bool CanAfford(int price)
    {
        if (GameData.instance == null ||
            GameData.instance.player == null)
        {
            Debug.LogError(
                "[ShopManager] 플레이어 데이터가 없습니다."
            );

            return false;
        }

        if (GameData.instance.player.Gold < price)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }

        return true;
    }

    private void SpendGold(int price)
    {
        GameData.instance.player.Gold -= price;
    }

    // =====================================================
    // 카드 구매
    // =====================================================

    private void TryBuyCard(
        ShopCardSlotUI slot,
        CardData card,
        int price)
    {
        if (slot == null ||
            slot.IsPurchased ||
            card == null ||
            !CanAfford(price))
        {
            return;
        }

        if (GameData.instance == null ||
            GameData.instance.player == null)
        {
            Debug.LogError(
                "[ShopManager] GameData 또는 Player 데이터가 없습니다."
            );

            return;
        }

        // 플레이어 덱에 카드 추가
        GameData.instance.player.deck.Add(
            new CardInstance(card)
        );

        // 덱 카드 수 갱신
        GameData.instance.player.DeckCount =
            GameData.instance.player.deck.Count;

        // 골드 차감
        SpendGold(price);

        // 구매한 상품 UI 숨기기
        slot.MarkPurchased();

        Debug.Log(
            $"카드 구매: {card.cardName}, 가격: {price}"
        );
    }

    // =====================================================
    // 유물 구매
    // =====================================================

    private void TryBuyRelic(
        ShopItemUI item,
        RelicData relic,
        int price)
    {
        if (item == null ||
            item.IsPurchased ||
            relic == null ||
            !CanAfford(price))
        {
            return;
        }

        RelicManager manager = RelicManager.EnsureInstance();

        if (manager == null)
        {
            return;
        }

        RelicInstance acquired = manager.Acquire(
            relic,
            RelicSource.Shop
        );

        // 유물 획득에 실패하면 골드와 상품을 유지한다.
        if (acquired == null)
        {
            return;
        }

        SpendGold(price);

        // 상품 UI만 숨기고 슬롯 공간은 유지한다.
        item.MarkPurchased();

        Debug.Log(
            $"유물 구매: {relic.DisplayName}, 가격: {price}"
        );
    }

    // =====================================================
    // 치즈 구매
    // =====================================================

    private void TryBuyCheese(
        ShopItemUI item,
        CheeseData cheese,
        int price)
    {
        if (item == null ||
            item.IsPurchased ||
            cheese == null ||
            !CanAfford(price))
        {
            return;
        }

        // 빈 슬롯이 있는지 먼저 확인한다.
        bool hasEmptySlot = false;

        foreach (CheeseData ownedCheese in
                 GameData.instance.CheeseInventory)
        {
            if (ownedCheese == null)
            {
                hasEmptySlot = true;
                break;
            }
        }

        if (!hasEmptySlot)
        {
            Debug.Log("치즈 인벤토리가 가득 찼습니다.");

            if (PersistentUI.Instance != null)
            {
                PersistentUI.Instance.ShowCheeseInventoryFullPopup();
            }

            return;
        }

        // 인벤토리에 실제로 추가된 경우에만 결제한다.
        if (!GameData.instance.AddCheese(cheese))
        {
            return;
        }

        SpendGold(price);

        // 상품 UI만 숨기고 슬롯 공간은 유지한다.
        item.MarkPurchased();

        if (PersistentUI.Instance != null)
        {
            PersistentUI.Instance.RefreshCheeseSlots();
        }

        Debug.Log(
            $"치즈 구매: {cheese.cheeseName}, 가격: {price}"
        );
    }

    // =====================================================
    // 상점 나가기
    // =====================================================

    private void LeaveShop()
    {
        if (SceneChanger.instance != null)
        {
            SceneChanger.instance.GoToMapScene();
        }
        else
        {
            Debug.LogError(
                "[ShopManager] SceneChanger가 없습니다."
            );
        }
    }
}