
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("Reward UI")]
    [SerializeField] private RewardPanelUI rewardPanelUI;

    private readonly List<Reward> rewards = new List<Reward>();

    public IReadOnlyList<Reward> Rewards => rewards;

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

    // 보상 UI 표시
    public void ShowRewards()
    {
        if (rewardPanelUI == null)
        {
            Debug.LogError(
                "RewardManager에 RewardPanelUI가 연결되지 않았습니다."
            );

            return;
        }

        rewardPanelUI.ShowRewards();
    }

    // 전투 유형에 따른 보상 생성
    public List<Reward> GenerateRewards()
    {
        rewards.Clear();

        if (GameData.instance == null)
        {
            Debug.LogError("GameData.instance가 없습니다.");
            return rewards;
        }

        switch (GameData.instance.CurrentBattleType)
        {
            case BattleType.Normal:
                GenerateNormalRewards();
                break;

            case BattleType.Elite:
                GenerateEliteRewards();
                break;

            case BattleType.Boss:
                GenerateBossRewards();
                break;
        }

        return rewards;
    }

    // 일반 전투 보상
    private void GenerateNormalRewards()
    {
        RewardSettings settings =
            GameData.instance.NormalEnemyReward;

        if (settings == null)
        {
            Debug.LogWarning(
                "NormalEnemyReward 설정이 없습니다."
            );

            return;
        }

        GenerateGold(settings);
        GenerateCheese(settings);

        GenerateCards(
            GameData.instance.CommonCardPool,
            settings.cardChoiceCount
        );
    }

    // 엘리트 전투 보상
    private void GenerateEliteRewards()
    {
        RewardSettings settings =
            GameData.instance.EliteEnemyReward;

        if (settings == null)
        {
            Debug.LogWarning(
                "EliteEnemyReward 설정이 없습니다."
            );

            return;
        }

        GenerateGold(settings);
        GenerateCheese(settings);

        GenerateCards(
            GameData.instance.RareCardPool,
            settings.cardChoiceCount
        );

        // TODO: 유물 시스템 구현 후 GenerateRelic() 호출
    }

    // 보스 전투 보상
    private void GenerateBossRewards()
    {
        // 현재는 미구현
    }

    // 골드 보상 생성
    private void GenerateGold(RewardSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        int minGold = Mathf.Min(
            settings.minGold,
            settings.maxGold
        );

        int maxGold = Mathf.Max(
            settings.minGold,
            settings.maxGold
        );

        int gold = Random.Range(
            minGold,
            maxGold + 1
        );

        rewards.Add(
            new Reward(
                RewardType.Gold,
                goldAmount: gold
            )
        );
    }

    // 치즈 보상 생성
    private void GenerateCheese(RewardSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        if (Random.value > settings.cheeseChance)
        {
            return;
        }

        rewards.Add(
            new Reward(
                RewardType.Cheese,
                cheeseHealAmount: settings.cheeseHealAmount
            )
        );
    }

    // 카드 선택지 생성
    private void GenerateCards(
        CardData[] pool,
        int choiceCount)
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning(
                "카드 보상 Pool이 비어 있습니다."
            );

            return;
        }

        if (choiceCount <= 0)
        {
            return;
        }

        List<CardData> candidates =
            new List<CardData>();

        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] == null)
            {
                continue;
            }

            if (!candidates.Contains(pool[i]))
            {
                candidates.Add(pool[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        int actualCount =
            Mathf.Min(choiceCount, candidates.Count);

        for (int i = 0; i < actualCount; i++)
        {
            int randomIndex =
                Random.Range(0, candidates.Count);

            CardData selectedCard =
                candidates[randomIndex];

            candidates.RemoveAt(randomIndex);

            rewards.Add(
                new Reward(
                    RewardType.Card,
                    cardData: selectedCard
                )
            );
        }
    }

    // 유물 보상 생성
    // 현재는 호출하지 않으며 유물 시스템 완성 후 사용
    //private void GenerateRelic()
    //{
    //    if (GameData.instance == null)
    //    {
    //        return;
    //    }

    //    RelicData[] pool =
    //        GameData.instance.RelicPool;

    //    if (pool == null || pool.Length == 0)
    //    {
    //        Debug.LogWarning(
    //            "Relic Pool이 비어 있습니다."
    //        );

    //        return;
    //    }

    //    if (RelicManager.Instance == null)
    //    {
    //        Debug.LogError(
    //            "RelicManager.Instance가 없습니다."
    //        );

    //        return;
    //    }

    //    List<RelicData> candidates =
    //        new List<RelicData>();

    //    for (int i = 0; i < pool.Length; i++)
    //    {
    //        RelicData relic = pool[i];

    //        if (relic == null)
    //        {
    //            continue;
    //        }

    //        if (RelicManager.Instance.HasRelic(
    //            relic.RelicId))
    //        {
    //            continue;
    //        }

    //        if (!candidates.Contains(relic))
    //        {
    //            candidates.Add(relic);
    //        }
    //    }

    //    if (candidates.Count == 0)
    //    {
    //        Debug.LogWarning(
    //            "현재 획득하지 않은 유물이 없습니다."
    //        );

    //        return;
    //    }

    //    int randomIndex =
    //        Random.Range(0, candidates.Count);

    //    RelicData selectedRelic =
    //        candidates[randomIndex];

    //    RelicReward relicReward =
    //        new RelicReward(
    //            selectedRelic,
    //            RelicSource.Elite
    //        );

    //    rewards.Add(
    //        new Reward(
    //            RewardType.Relic,
    //            relicReward: relicReward
    //        )
    //    );
    //}
}
