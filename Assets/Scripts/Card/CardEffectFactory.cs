
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


            // Field Mouse Bandit
            case CardEffectType.ClawSwipe:
                return new ClawSwipeEffect(card);

            case CardEffectType.DustCloud:
                return new DustCloudEffect(card);

            case CardEffectType.StoneThrow:
                return new StoneThrowEffect(card);

            case CardEffectType.TakeOver:
                return new TakeOverEffect(card);

            // Green Slime
            case CardEffectType.GreenSlime_BodySlam:
                return new GreenSlime_BodySlamEffect(card);

            case CardEffectType.SlimeShield:
                return new SlimeShieldEffect(card);

            case CardEffectType.SlimeShot:
                return new SlimeShotEffect(card);

            case CardEffectType.StickySlime:
                return new StickySlimeEffect(card);

            // Ember Boar
            case CardEffectType.Charge:
                return new ChargeEffect(card);

            case CardEffectType.TuskSwing:
                return new TuskSwingEffect(card);

            case CardEffectType.BlazingCharge:
                return new BlazingChargeEffect(card);

            case CardEffectType.GroundStomp:
                return new GroundStompEffect(card);

            case CardEffectType.BlazingCounter:
                return new BlazingCounterEffect(card);

            case CardEffectType.SavageRush:
                return new SavageRushEffect(card);

            // Ironback Beetle
            case CardEffectType.PincerStrike:
                return new PincerStrikeEffect(card);

            case CardEffectType.ShellGuard:
                return new ShellGuardEffect(card);

            case CardEffectType.ShieldBash:
                return new ShieldBashEffect(card);

            case CardEffectType.CrushingPincer:
                return new CrushingPincerEffect(card);

            case CardEffectType.IronStance:
                return new IronStanceEffect(card);

            case CardEffectType.SpinningShell:
                return new SpinningShellEffect(card);

            // Mushroom Shaman
            case CardEffectType.SporeShot:
                return new SporeShotEffect(card);

            case CardEffectType.BlindingSpores:
                return new BlindingSporesEffect(card);

            case CardEffectType.VulnerabilitySpores:
                return new VulnerabilitySporesEffect(card);

            case CardEffectType.RottingBurst:
                return new RottingBurstEffect(card);

            case CardEffectType.MyceliumBarrier:
                return new MyceliumBarrierEffect(card);

            case CardEffectType.GiantSporeBurst:
                return new GiantSporeBurstEffect(card);

            default:
                Debug.Log("Unknown card type!");
                return null;
        }
    }
}
