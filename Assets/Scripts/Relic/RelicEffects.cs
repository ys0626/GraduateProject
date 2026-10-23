using System.Collections.Generic;
using UnityEngine;
using static RelicEffectUtility;

/// <summary>전투 시작 후 힘을 1 얻는다.</summary>
public sealed class StartBattleStrengthRelicEffect : IRelicEffect
{
    private const int StrengthAmount = 1;

    public void OnAcquired(RelicInstance relic) { }

    public void OnBattleStarted(Entity owner, Entity opponent)
    {
        if (owner != null)
            owner.AddStatus(new StrengthStatus(StrengthAmount));
    }

    public void OnTurnStarted(Entity owner) { }
    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>전투 시작 후 방어도를 10 얻는다.</summary>
public sealed class StartBattleBlockRelicEffect : IRelicEffect
{
    private const int BlockAmount = 10;

    public void OnAcquired(RelicInstance relic) { }

    public void OnBattleStarted(Entity owner, Entity opponent)
    {
        if (owner != null)
            owner.GetBlock(BlockAmount);
    }

    public void OnTurnStarted(Entity owner) { }
    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>획득 즉시 이번 런의 최대 체력을 7 늘린다.</summary>
public sealed class MaxHealthRelicEffect : IRelicEffect
{
    private const int MaxHealthAmount = 7;

    public void OnAcquired(RelicInstance relic)
    {
        if (GameData.instance == null)
        {
            Debug.LogWarning("최대 체력 유물 효과를 적용하지 못했습니다: GameData가 없습니다.");
            return;
        }

        GameData.instance.player.MaxHP += MaxHealthAmount;
        GameData.instance.player.Heal(MaxHealthAmount);
    }

    public void OnBattleStarted(Entity owner, Entity opponent) { }
    public void OnTurnStarted(Entity owner) { }
    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>매 전투에서 처음 사용하는 공격 카드에 피해 8을 더한다.</summary>
public sealed class FirstAttackBonusDamageRelicEffect : IRelicEffect
{
    private const int BonusDamage = 8;
    private bool hasUsedAttackThisBattle;

    public void OnAcquired(RelicInstance relic) { }

    public void OnBattleStarted(Entity owner, Entity opponent)
    {
        hasUsedAttackThisBattle = false;
    }

    public void OnTurnStarted(Entity owner) { }

    public void OnCardPlayed(Entity owner, CardInstance card, Entity target)
    {
        if (hasUsedAttackThisBattle || !IsPlayer(owner) || card?.data == null ||
            card.data.cardType != CardType.Attack || target == null)
        {
            return;
        }

        hasUsedAttackThisBattle = true;
        target.TakeDamage(owner.CalculateDamage(BonusDamage));
    }

    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>턴 종료 시 방어도가 없으면 방어도 6을 얻는다.</summary>
public sealed class EndTurnBlockIfUnarmoredRelicEffect : IRelicEffect
{
    private const int BlockAmount = 6;

    public void OnAcquired(RelicInstance relic) { }
    public void OnBattleStarted(Entity owner, Entity opponent) { }
    public void OnTurnStarted(Entity owner) { }
    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }

    public void OnTurnEnded(Entity owner)
    {
        if (IsPlayer(owner) && owner.Block == 0)
            owner.GetBlock(BlockAmount);
    }

    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>전투 시작 후 첫 번째 플레이어 턴에 에너지 1을 추가한다.</summary>
public sealed class FirstTurnBonusEnergyRelicEffect : IRelicEffect
{
    private const int EnergyAmount = 1;
    private bool firstTurnPending;

    public void OnAcquired(RelicInstance relic) { }

    public void OnBattleStarted(Entity owner, Entity opponent)
    {
        firstTurnPending = true;
    }

    public void OnTurnStarted(Entity owner)
    {
        if (!firstTurnPending || !IsPlayer(owner))
            return;

        owner.CurrentEnergy += EnergyAmount;
        firstTurnPending = false;
    }

    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>세 번째 플레이어 턴마다 에너지 1을 추가한다.</summary>
public sealed class EveryThirdTurnBonusEnergyRelicEffect : IRelicEffect
{
    private const int EnergyAmount = 1;
    private int playerTurnCount;

    public void OnAcquired(RelicInstance relic) { }

    public void OnBattleStarted(Entity owner, Entity opponent)
    {
        playerTurnCount = 0;
    }

    public void OnTurnStarted(Entity owner)
    {
        if (!IsPlayer(owner))
            return;

        playerTurnCount++;
        if (playerTurnCount % 3 == 0)
            owner.CurrentEnergy += EnergyAmount;
    }

    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>파워 카드 사용 시 손패의 비용이 있는 카드 하나를 이번 턴 동안 0으로 만든다.</summary>
public sealed class PowerCardRandomDiscountRelicEffect : IRelicEffect
{
    private readonly List<CardInstance> discountedCards = new List<CardInstance>();

    public void OnAcquired(RelicInstance relic) { }
    public void OnBattleStarted(Entity owner, Entity opponent) { }
    public void OnTurnStarted(Entity owner) { }

    public void OnCardPlayed(Entity owner, CardInstance card, Entity target)
    {
        if (!IsPlayer(owner) || card?.data == null ||
            card.data.cardType != CardType.Power)
        {
            return;
        }

        List<CardInstance> candidates = owner.hand.FindAll(handCard =>
            handCard != null && handCard.currentCost > 0);

        if (candidates.Count == 0)
            return;

        CardInstance selected = candidates[Random.Range(0, candidates.Count)];
        selected.currentCost = 0;
        discountedCards.Add(selected);
        RefreshCardUI(selected);
    }

    public void OnTurnEnded(Entity owner)
    {
        if (!IsPlayer(owner))
            return;

        foreach (CardInstance discountedCard in discountedCards)
        {
            if (discountedCard != null && discountedCard.data != null)
            {
                discountedCard.currentCost = discountedCard.data.cost;
                RefreshCardUI(discountedCard);
            }
        }

        discountedCards.Clear();
    }

    public void OnBattleEnded(bool playerWon)
    {
        discountedCards.Clear();
    }
}

/// <summary>두 번째 플레이어 턴 시작 시 방어도 14를 얻는다.</summary>
public sealed class SecondTurnBlockRelicEffect : IRelicEffect
{
    private const int BlockAmount = 14;
    private int playerTurnCount;

    public void OnAcquired(RelicInstance relic) { }

    public void OnBattleStarted(Entity owner, Entity opponent)
    {
        playerTurnCount = 0;
    }

    public void OnTurnStarted(Entity owner)
    {
        if (!IsPlayer(owner))
            return;

        playerTurnCount++;
        if (playerTurnCount == 2)
            owner.GetBlock(BlockAmount);
    }

    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

/// <summary>플레이어 턴 시작 시 적에게 피해 5를 준다.</summary>
public sealed class TurnStartDamageRelicEffect : IRelicEffect
{
    private const int DamageAmount = 5;

    public void OnAcquired(RelicInstance relic) { }
    public void OnBattleStarted(Entity owner, Entity opponent) { }

    public void OnTurnStarted(Entity owner)
    {
        if (!IsPlayer(owner) || GameData.instance == null)
            return;

        GameData.instance.enemy.TakeDamage(owner.CalculateDamage(DamageAmount));
    }

    public void OnCardPlayed(Entity owner, CardInstance card, Entity target) { }
    public void OnTurnEnded(Entity owner) { }
    public void OnBattleEnded(bool playerWon) { }
}

internal static class RelicEffectUtility
{
    public static bool IsPlayer(Entity owner)
    {
        return GameData.instance != null && owner == GameData.instance.player;
    }

    public static void RefreshCardUI(CardInstance card)
    {
        if (HandManager.instance == null)
            return;

        CardView view = HandManager.instance.cards.Find(cardView => cardView.card == card);
        if (view != null)
            view.GetComponent<CardUI>()?.RefreshUI();
    }
}
