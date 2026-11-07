
using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardEffectFactory
{
    private static readonly Dictionary<string, Func<CardInstance, ICardEffect>> effectMap
        = new Dictionary<string, Func<CardInstance, ICardEffect>>()
    {
        // =========================================================
        // ENEMY CARDS
        // =========================================================

        // Field Mouse Bandit
        { "ClawSwipe", card => new ClawSwipeEffect(card) },
        { "DustCloud", card => new DustCloudEffect(card) },
        { "StoneThrow", card => new StoneThrowEffect(card) },
        { "TakeOver", card => new TakeOverEffect(card) },

        // Green Slime
        { "GreenSlime_BodySlam", card => new GreenSlime_BodySlamEffect(card) },
        { "SlimeShield", card => new SlimeShieldEffect(card) },
        { "SlimeShot", card => new SlimeShotEffect(card) },
        { "StickySlime", card => new StickySlimeEffect(card) },

        // Ember Boar
        { "Charge", card => new ChargeEffect(card) },
        { "TuskSwing", card => new TuskSwingEffect(card) },
        { "BlazingCharge", card => new BlazingChargeEffect(card) },
        { "GroundStomp", card => new GroundStompEffect(card) },
        { "BlazingCounter", card => new BlazingCounterEffect(card) },
        { "SavageRush", card => new SavageRushEffect(card) },

        // Ironback Beetle
        { "PincerStrike", card => new PincerStrikeEffect(card) },
        { "ShellGuard", card => new ShellGuardEffect(card) },
        { "IronbackBeetle_ShieldBash", card => new IronbackBeetle_ShieldBashEffect(card) },
        { "CrushingPincer", card => new CrushingPincerEffect(card) },
        { "IronStance", card => new IronStanceEffect(card) },
        { "SpinningShell", card => new SpinningShellEffect(card) },

        // Mushroom Shaman
        { "SporeShot", card => new SporeShotEffect(card) },
        { "BlindingSpores", card => new BlindingSporesEffect(card) },
        { "VulnerabilitySpores", card => new VulnerabilitySporesEffect(card) },
        { "RottingBurst", card => new RottingBurstEffect(card) },
        { "MyceliumBarrier", card => new MyceliumBarrierEffect(card) },
        { "GiantSporeBurst", card => new GiantSporeBurstEffect(card) },

        // Corrupted Knight
        { "BattleFervor", card => new BattleFervorEffect(card) },
        { "ShieldAccumulation", card => new ShieldAccumulationEffect(card) },
        { "CorruptedKnight_ShieldBash", card => new CorruptedKnight_ShieldBashEffect(card) },
        { "DoubleStrike", card => new DoubleStrikeEffect(card) },
        { "IroncladStance", card => new IroncladStanceEffect(card) },
        { "CrushingBlow", card => new CrushingBlowEffect(card) },

        // Crystal Falconer
        { "WindSense", card => new WindSenseEffect(card) },
        { "EvasiveStance", card => new EvasiveStanceEffect(card) },
        { "RapidFire", card => new RapidFireEffect(card) },
        { "FeatherBarrier", card => new FeatherBarrierEffect(card) },
        { "MarkedArrow", card => new MarkedArrowEffect(card) },
        { "GaleFlurry", card => new GaleFlurryEffect(card) },
        { "RetreatingLeap", card => new RetreatingLeapEffect(card) },

        // Ancient Doom Dragon
        { "WarInstinct", card => new WarInstinctEffect(card) },
        { "EvasionInstinct", card => new EvasionInstinctEffect(card) },
        { "ScaleBarrier", card => new ScaleBarrierEffect(card) },
        { "DragonBarrage", card => new DragonBarrageEffect(card) },
        { "DefenseConversion", card => new DefenseConversionEffect(card) },
        { "RendingClaw", card => new RendingClawEffect(card) },
        { "ApocalypticRoar", card => new ApocalypticRoarEffect(card) },
        { "AncientScales", card => new AncientScalesEffect(card) },

        // =========================================================
        // PLAYER CARDS
        // =========================================================

        // Start Cards
        { "Bash", card => new BashEffect(card) },
        { "Defend", card => new DefendEffect(card) },
        { "Strike", card => new StrikeEffect(card) },

        // Common Attack Cards
        { "HeavyStrike", card => new HeavyStrikeEffect(card) },
        { "PiercingThrust", card => new PiercingThrustEffect(card) },
        { "DoubleSlash", card => new DoubleSlashEffect(card) },
        { "HeavyBlow", card => new HeavyBlowEffect(card) },
        { "ExploitWeakness", card => new ExploitWeaknessEffect(card) },
        { "CrushingAdvance", card => new CrushingAdvanceEffect(card) },
        { "QuickJab", card => new QuickJabEffect(card) },
        { "FuriousStrike", card => new FuriousStrikeEffect(card) },
        { "ShieldBash", card => new ShieldBashEffect(card) },
        { "Rend", card => new RendEffect(card) },
        { "DesperateStrike", card => new DesperateStrikeEffect(card) },
        { "SavageStrike", card => new SavageStrikeEffect(card) },
        { "Counterattack", card => new CounterattackEffect(card) },
        { "Shatter", card => new ShatterEffect(card) },

        // Common Skill Cards
        { "IronWall", card => new IronWallEffect(card) },
        { "DefensiveStance", card => new DefensiveStanceEffect(card) },
        { "EmergencyBarrier", card => new EmergencyBarrierEffect(card) },
        { "SteadfastGuard", card => new SteadfastGuardEffect(card) },
        { "WeakenedCry", card => new WeakenedCryEffect(card) },
        { "ExposeWeakness", card => new ExposeWeaknessEffect(card) },
        { "RallyingDefense", card => new RallyingDefenseEffect(card) },
        { "RaiseShield", card => new RaiseShieldEffect(card) },
        { "SurvivalInstinct", card => new SurvivalInstinctEffect(card) },
        { "Phalanx", card => new PhalanxEffect(card) },
        { "DisorientingBlow", card => new DisorientingBlowEffect(card) },
        { "LastStand", card => new LastStandEffect(card) },

        // Common Power Cards
        { "StrengthTraining", card => new StrengthTrainingEffect(card) },
        { "FleetFootwork", card => new FleetFootworkEffect(card) },
        { "CombatManual", card => new CombatManualEffect(card) },
        { "IronResolve", card => new IronResolveEffect(card) },

        // Rare Attack Cards
        { "StormOfBlades", card => new StormOfBladesEffect(card) },
        { "Execution", card => new ExecutionEffect(card) },
        { "Riftbreaker", card => new RiftbreakerEffect(card) },
        { "SoulHarvest", card => new SoulHarvestEffect(card) },

        // Rare skill cards
        { "ImpenetrableFortress", card => new ImpenetrableFortressEffect(card) },
        { "WavesOfDefense", card => new WavesOfDefenseEffect(card) },
        { "FortressAmplification", card => new FortressAmplificationEffect(card) },

        // Rare Power Cards
        { "UnyieldingBulwark", card => new UnyieldingBulwarkEffect(card) },
        { "EndlessStruggle", card => new EndlessStruggleEffect(card) },
        { "PerfectArmament", card => new PerfectArmamentEffect(card) }
    };

    public static ICardEffect Create(CardInstance card)
    {
        if (card == null || card.data == null)
        {
            Debug.LogError("CardEffectFactory: Card 또는 CardData가 null입니다.");
            return null;
        }

        string effectId = card.data.name;

        if (effectMap.TryGetValue(effectId, out Func<CardInstance, ICardEffect> createEffect))
        {
            return createEffect(card);
        }

        Debug.LogError($"CardEffectFactory: 등록되지 않은 카드 효과입니다. CardData 이름: {effectId}");
        return null;
    }
}
