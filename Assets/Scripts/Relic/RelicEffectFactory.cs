
// RelicEffectType을 실제 IRelicEffect 구현으로 연결하는 진입점
// 현재는 효과가 없으므로 null을 반환하며, 효과 제작 시 switch 문을 확장

public static class RelicEffectFactory
{
    public static IRelicEffect Create(RelicInstance relic)
    {
        if (relic == null || relic.Data == null)
        {
            return null;
        }

        switch (relic.Data.EffectType)
        {
            case RelicEffectType.StartBattleStrength:
                return new StartBattleStrengthRelicEffect();

            case RelicEffectType.StartBattleBlock:
                return new StartBattleBlockRelicEffect();

            case RelicEffectType.MaxHealthOnAcquired:
                return new MaxHealthRelicEffect();

            case RelicEffectType.FirstAttackBonusDamage:
                return new FirstAttackBonusDamageRelicEffect();

            case RelicEffectType.EndTurnBlockIfUnarmored:
                return new EndTurnBlockIfUnarmoredRelicEffect();

            case RelicEffectType.FirstTurnBonusEnergy:
                return new FirstTurnBonusEnergyRelicEffect();

            case RelicEffectType.EveryThirdTurnBonusEnergy:
                return new EveryThirdTurnBonusEnergyRelicEffect();

            case RelicEffectType.PowerCardRandomDiscount:
                return new PowerCardRandomDiscountRelicEffect();

            case RelicEffectType.SecondTurnBlock:
                return new SecondTurnBlockRelicEffect();

            case RelicEffectType.TurnStartDamage:
                return new TurnStartDamageRelicEffect();

            case RelicEffectType.None:
            default:
                return null;
        }
    }
}
