using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점에 진열할 카드/유물을 랜덤으로 뽑고 가격을 계산
/// </summary>
public static class ShopGenerator
{
    // =====================================================
    // 설정값
    // =====================================================

    private const int CARD_ENTRY_COUNT = 5; // 상점에 진열되는 카드 수

    private const int COMMON_PRICE = 50;
    private const int UNCOMMON_PRICE = 75;
    private const int RARE_PRICE = 120;

    private const int RELIC_PRICE = 150;
    // 배틀 승리 후 얼마를 얻을 수 있는지에 따라 가격을 조정할 수 있음

    // =====================================================
    // 카드 진열
    // =====================================================

    /// <summary>
    /// 카드 풀에서 중복 없이 카드를 랜덤으로 뽑아 진열 목록 생성
    /// </summary>
    public static List<ShopEntry> GenerateCardEntries(List<CardData> cardPool)
    {
        List<ShopEntry> entries =
            new List<ShopEntry>();

        if (cardPool == null || cardPool.Count == 0)
        {
            return entries;
        }

        List<CardData> pool =
            new List<CardData>(cardPool);

        int count =
            Mathf.Min(CARD_ENTRY_COUNT, pool.Count); // 진열할 카드 수는 풀의 크기보다 클 수 없음

        for (int i = 0; i < count; i++)
        {
            int randomIndex =
                Random.Range(0, pool.Count);

            CardData selected = pool[randomIndex];

            pool.RemoveAt(randomIndex);

            ShopEntry entry = new ShopEntry
            {
                card = selected,
                relic = null,
                price = GetCardPrice(selected.rarity),
                purchased = false
            };

            entries.Add(entry);
        }

        return entries;
    }

    // =====================================================
    // 유물 진열
    // =====================================================

    /// <summary>
    /// 유물 풀에서 랜덤으로 1개 뽑아 진열 항목 생성
    /// </summary>
    public static ShopEntry GenerateRelicEntry(List<RelicData> relicPool)
    {
        if (relicPool == null || relicPool.Count == 0)
        {
            return null;
        }

        int randomIndex =
            Random.Range(0, relicPool.Count);

        RelicData selected = relicPool[randomIndex];

        return new ShopEntry
        {
            card = null,
            relic = selected,
            price = GetRelicPrice(),
            purchased = false
        };
    }

    // =====================================================
    // 가격 계산
    // =====================================================

    /// <summary>
    /// 희귀도에 따른 카드 가격 반환
    /// </summary>
    private static int GetCardPrice(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return COMMON_PRICE;

            case CardRarity.Uncommon:
                return UNCOMMON_PRICE;

            case CardRarity.Rare:
                return RARE_PRICE;

            default:
                return COMMON_PRICE;
        }
    }

    /// <summary>
    /// 유물 가격 반환
    /// </summary>
    private static int GetRelicPrice()
    {
        return RELIC_PRICE;
    }
}
