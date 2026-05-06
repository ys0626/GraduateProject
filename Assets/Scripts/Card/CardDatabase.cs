using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 카드 목록 관리 (ScriptableObject)
/// CardData ScriptableObject를 직접 참조
/// </summary>
[CreateAssetMenu(fileName = "CardDatabase", menuName = "Scriptable Objects/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    [Header("Card List")]
    public List<CardEntry> cards;

    [System.Serializable]
    public class CardEntry
    {
        public string cardId;   // 고유 ID (ex: "attack_001")
        public CardData cardData; // CardData ScriptableObject 직접 참조
    }

    // ─────────────────────────────────────────────
    // 조회 메서드
    // ─────────────────────────────────────────────

    /// <summary>
    /// ID로 CardData 반환
    /// </summary>
    public CardData GetCardById(string id)
    {
        CardEntry entry = cards.Find(c => c.cardId == id);
        if (entry == null)
        {
            Debug.LogWarning($"[CardDatabase] ID '{id}'에 해당하는 카드 없음");
            return null;
        }
        return entry.cardData;
    }

    /// <summary>
    /// 이름으로 CardData 반환
    /// </summary>
    public CardData GetCardByName(string cardName)
    {
        CardEntry entry = cards.Find(c => c.cardData != null
                                       && c.cardData.cardName == cardName);
        if (entry == null)
        {
            Debug.LogWarning($"[CardDatabase] 이름 '{cardName}'에 해당하는 카드 없음");
            return null;
        }
        return entry.cardData;
    }

    /// <summary>
    /// 카드 타입으로 목록 반환
    /// </summary>
    public List<CardData> GetCardsByType(CardType type)
    {
        List<CardData> result = new List<CardData>();
        foreach (CardEntry entry in cards)
        {
            if (entry.cardData != null && entry.cardData.cardType == type)
                result.Add(entry.cardData);
        }
        return result;
    }

    /// <summary>
    /// 희귀도로 목록 반환
    /// </summary>
    public List<CardData> GetCardsByRarity(CardRarity rarity)
    {
        List<CardData> result = new List<CardData>();
        foreach (CardEntry entry in cards)
        {
            if (entry.cardData != null && entry.cardData.rarity == rarity)
                result.Add(entry.cardData);
        }
        return result;
    }
}
