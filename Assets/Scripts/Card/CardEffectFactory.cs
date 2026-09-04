using UnityEngine;
public static class CardEffectFactory
{
    public static ICardEffect Create(CardInstance card)
    {
        switch (card.data.cardEffectType)
        {
            case CardEffectType.Bash:
                return new BashEffect(card);

            case CardEffectType.BodySlam:
                return new BodySlamEffect(card);

            case CardEffectType.Dash:
                return new DashEffect(card);

            case CardEffectType.Defend:
                return new DefendEffect(card);

            case CardEffectType.DoubleTap:
                return new DoubleTapEffect();

            case CardEffectType.Entrench:
                return new EntrenchEffect(card);

            case CardEffectType.Footwork:
                return new FootworkEffect(card);

            case CardEffectType.Inflame:
                return new InflameEffect(card);

            case CardEffectType.LimitBreak:
                return new LimitBreakEffect();

            case CardEffectType.Strike:
                return new StrikeEffect(card);

            case CardEffectType.SwordBoomerang:
                return new SwordBoomerangEffect(card);

            case CardEffectType.Taunt:
                return new TauntEffect(card);

            case CardEffectType.TwinStrike:
                return new TwinStrikeEffect(card);

            case CardEffectType.Uppercut:
                return new UppercutEffect(card);

            default:
                Debug.Log("Unknown card type!");
                return null;
        }
    }
}