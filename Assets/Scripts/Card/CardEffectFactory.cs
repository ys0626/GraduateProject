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
                Debug.Log("Unknown card type!");
                return null;
        }
    }
}