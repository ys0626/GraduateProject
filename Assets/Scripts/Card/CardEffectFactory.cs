using UnityEngine;

public static class CardEffectFactory
{
    public static ICardEffect Create(CardInstance card)
    {
        switch (card.data.cardEffectType)
        {
            case CardEffectType.Strike:
                return new StrikeEffect(card);

            case CardEffectType.Defend:
                return new DefendEffect(card);

            default:
                Debug.LogWarning($"[CardEffectFactory] 미구현 카드 타입: {card.data.cardEffectType}");
                return null;
        }
    }
}
