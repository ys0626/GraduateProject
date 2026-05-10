using UnityEngine;
public static class CardEffectFactory
{
    public static ICardEffect Create(CardInstance card)
    {
        switch (card.data.cardEffectType)
        {
            case CardEffectType.Bash:
                return new BashEffect(card);

            case CardEffectType.Defend:
                return new DefendEffect(card);

            case CardEffectType.Footwork:
                return new FootworkEffect(card);

            case CardEffectType.Inflame:
                return new InflameEffect(card);

            case CardEffectType.Strike:
                return new StrikeEffect(card);
            
            case CardEffectType.Uppercut:
                return new UppercutEffect(card);

            default:
                Debug.Log("Unknown card type!");
                return null;
        }
    }
}