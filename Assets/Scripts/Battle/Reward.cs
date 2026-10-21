public class Reward
{
    public RewardType RewardType { get; }

    public int GoldAmount { get; }

    public CheeseData CheeseData { get; }

    public CardData CardData { get; }

    public RelicReward RelicReward { get; }

    public Reward(
        RewardType rewardType,
        int goldAmount = 0,
        CheeseData cheeseData = null,
        CardData cardData = null,
        RelicReward relicReward = null)
    {
        RewardType = rewardType;
        GoldAmount = goldAmount;
        CheeseData = cheeseData;
        CardData = cardData;
        RelicReward = relicReward;
    }
}