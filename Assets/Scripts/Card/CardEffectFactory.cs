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

            //Field Mouse Bandit
            case CardEffectType.ClawSwipe:
                return new ClawSwipeEffect(card);
            case CardEffectType.DustCloud:
                return new DustCloudEffect(card);
            case CardEffectType.StoneThrow:
                return new StoneThrowEffect(card);
            case CardEffectType.TakeOver:
                return new TakeOverEffect(card);

            //Green Slime
            case CardEffectType.GreenSlime_BodySlam:
                return new GreenSlime_BodySlamEffect(card);
            case CardEffectType.SlimeShield:
                return new SlimeShieldEffect(card);
            case CardEffectType.SlimeShot:
                return new SlimeShotEffect(card);
            case CardEffectType.StickySlime:
                return new StickySlimeEffect(card);

            default:
                Debug.Log("Unknown card type!");
                return null;
        }
    }
}