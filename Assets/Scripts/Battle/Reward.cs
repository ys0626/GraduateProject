public class Reward
{
    public RewardType RewardType { get; }

    public int GoldAmount { get; }

    public int CheeseHealAmount { get; }

    public CardData CardData { get; }

    public RelicReward RelicReward { get; }

    public Reward(
        RewardType rewardType,
        int goldAmount = 0,
        int cheeseHealAmount = 0,
        CardData cardData = null,
        RelicReward relicReward = null)
    {
        RewardType = rewardType;
        GoldAmount = goldAmount;
        CheeseHealAmount = cheeseHealAmount;
        CardData = cardData;
        RelicReward = relicReward;
    }
}