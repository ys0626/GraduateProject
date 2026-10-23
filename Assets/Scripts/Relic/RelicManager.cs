using System;
using System.Collections.Generic;
using UnityEngine;

// 이번 런의 유물 보유 목록과 획득·효과 호출을 관리
// 상점과 이벤트는 이 클래스를 통해서만 유물을 지급하도록 연결
// 새 런 또는 게임오버 처리 시 ClearRunRelics를 호출하는 구조를 사용

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    private readonly List<RelicInstance> relics = new List<RelicInstance>();
    private readonly Dictionary<RelicInstance, IRelicEffect> effects =
        new Dictionary<RelicInstance, IRelicEffect>();

    public IReadOnlyList<RelicInstance> Relics => relics;

    public event Action<RelicInstance> OnRelicAcquired;
    public event Action OnRelicsChanged;

    private readonly HashSet<string> appearedRelicIds =
    new HashSet<string>();

    // 유물은 전투 씬이 바뀌어도 게임오버까지 유지되어야 한다.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>상점과 이벤트가 유물을 지급할 때 사용할 관리자를 보장한다.</summary>
    public static RelicManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject managerObject = new GameObject(nameof(RelicManager));
        return managerObject.AddComponent<RelicManager>();
    }

    // 상점 또는 이벤트 보상으로 유물을 획득
    public RelicInstance Acquire(RelicData relicData, RelicSource source)
    {
        if (relicData == null)
        {
            Debug.LogError("유물 획득에 실패했습니다: RelicData가 없습니다.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(relicData.RelicId))
        {
            Debug.LogError($"유물 획득에 실패했습니다: '{relicData.name}'의 RelicId가 비어 있습니다.");
            return null;
        }

        appearedRelicIds.Add(relicData.RelicId);

        RelicInstance relic = new RelicInstance(relicData, source);
        if (HasRelic(relicData.RelicId))
        {
            Debug.LogWarning(
                $"이미 보유한 유물입니다: {relicData.DisplayName}");

            return null;
        }
        relics.Add(relic);

        IRelicEffect effect = RelicEffectFactory.Create(relic);
        if (effect != null)
        {
            effects.Add(relic, effect);
            effect.OnAcquired(relic);
        }

        OnRelicAcquired?.Invoke(relic);
        OnRelicsChanged?.Invoke();
        Debug.Log($"유물 획득: {relicData.DisplayName} ({source})");
        return relic;
    }

    // 해당 유물을 현재 런에서 보유했는지 확인
    public bool HasRelic(string relicId)
    {
        if (string.IsNullOrWhiteSpace(relicId))
        {
            return false;
        }

        foreach (RelicInstance relic in relics)
        {
            if (relic.Data != null && relic.Data.RelicId == relicId)
            {
                return true;
            }
        }

        return false;
    }

    // 게임오버 또는 새 런 시작 시 모든 보유 유물을 제거
    public void ClearRunRelics()
    {
        effects.Clear();
        relics.Clear();
        appearedRelicIds.Clear();

        OnRelicsChanged?.Invoke();
    }

    // BattleManager가 전투 시작 시 호출할 유물
    public void NotifyBattleStarted(Entity owner, Entity opponent)
    {
        foreach (RelicInstance relic in relics)
        {
            if (effects.TryGetValue(relic, out IRelicEffect effect))
            {
                effect.OnBattleStarted(owner, opponent);
            }
        }
    }

    // BattleManager가 보유자의 턴 시작 시 호출할 유물 훅
    public void NotifyTurnStarted(Entity owner)
    {
        foreach (RelicInstance relic in relics)
        {
            if (effects.TryGetValue(relic, out IRelicEffect effect))
            {
                effect.OnTurnStarted(owner);
            }
        }
    }

    // BattleManager가 카드 사용 성공 후 호출할 유물 
    public void NotifyCardPlayed(Entity owner, CardInstance card, Entity target)
    {
        foreach (RelicInstance relic in relics)
        {
            if (effects.TryGetValue(relic, out IRelicEffect effect))
            {
                effect.OnCardPlayed(owner, card, target);
            }
        }
    }

    // BattleManager가 보유자의 턴 종료 시 호출할 유물 훅
    public void NotifyTurnEnded(Entity owner)
    {
        foreach (RelicInstance relic in relics)
        {
            if (effects.TryGetValue(relic, out IRelicEffect effect))
            {
                effect.OnTurnEnded(owner);
            }
        }
    }

    // BattleManager가 전투 종료 시 호출할 유물
    public void NotifyBattleEnded(bool playerWon)
    {
        foreach (RelicInstance relic in relics)
        {
            if (effects.TryGetValue(relic, out IRelicEffect effect))
            {
                effect.OnBattleEnded(playerWon);
            }
        }
    }

    public List<RelicData> DrawRandomAvailableRelics(int count)
    {
        List<RelicData> candidates = new List<RelicData>();
        List<RelicData> results = new List<RelicData>();

        if (GameData.instance == null)
        {
            Debug.LogError("유물 추첨 실패: GameData가 없습니다.");
            return results;
        }

        foreach (RelicData relic in GameData.instance.RelicPool)
        {
            if (relic == null)
                continue;

            if (string.IsNullOrWhiteSpace(relic.RelicId))
                continue;

            if (appearedRelicIds.Contains(relic.RelicId))
                continue;

            candidates.Add(relic);
        }

        count = Mathf.Min(count, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            RelicData selected = candidates[index];

            results.Add(selected);
            appearedRelicIds.Add(selected.RelicId);

            candidates.RemoveAt(index);
        }

        return results;
    }








    // =========================
    // Relic Test Context Menus
    // =========================

    private void TestAcquireRelic(RelicEffectType effectType)
    {
        if (GameData.instance == null)
        {
            Debug.LogWarning("GameData 인스턴스가 없습니다.");
            return;
        }

        RelicData targetRelic = null;

        foreach (RelicData relic in GameData.instance.RelicPool)
        {
            if (relic != null && relic.EffectType == effectType)
            {
                targetRelic = relic;
                break;
            }
        }

        if (targetRelic == null)
        {
            Debug.LogWarning(
                "해당 효과 타입의 유물을 RelicPool에서 찾을 수 없습니다: "
                + effectType);
            return;
        }

        RelicInstance result = Acquire(targetRelic, RelicSource.Event);

        if (result != null)
        {
            Debug.Log("테스트 유물 획득: " + targetRelic.DisplayName);
        }
        else
        {
            Debug.LogWarning(
                "유물을 획득하지 못했습니다. 이미 보유 중인지 확인하세요: "
                + targetRelic.DisplayName);
        }
    }

    [ContextMenu("Relic Test/1. Start Battle Strength")]
    private void TestStartBattleStrength()
    {
        TestAcquireRelic(RelicEffectType.StartBattleStrength);
    }

    [ContextMenu("Relic Test/2. Start Battle Block")]
    private void TestStartBattleBlock()
    {
        TestAcquireRelic(RelicEffectType.StartBattleBlock);
    }

    [ContextMenu("Relic Test/3. Max Health On Acquired")]
    private void TestMaxHealthOnAcquired()
    {
        TestAcquireRelic(RelicEffectType.MaxHealthOnAcquired);
    }

    [ContextMenu("Relic Test/4. End Turn Block If Unarmored")]
    private void TestEndTurnBlockIfUnarmored()
    {
        TestAcquireRelic(RelicEffectType.EndTurnBlockIfUnarmored);
    }

    [ContextMenu("Relic Test/5. First Turn Bonus Energy")]
    private void TestFirstTurnBonusEnergy()
    {
        TestAcquireRelic(RelicEffectType.FirstTurnBonusEnergy);
    }

    [ContextMenu("Relic Test/6. Every Third Turn Bonus Energy")]
    private void TestEveryThirdTurnBonusEnergy()
    {
        TestAcquireRelic(RelicEffectType.EveryThirdTurnBonusEnergy);
    }

    [ContextMenu("Relic Test/7. Second Turn Block")]
    private void TestSecondTurnBlock()
    {
        TestAcquireRelic(RelicEffectType.SecondTurnBlock);
    }

    [ContextMenu("Relic Test/8. Turn Start Damage")]
    private void TestTurnStartDamage()
    {
        TestAcquireRelic(RelicEffectType.TurnStartDamage);
    }

}
