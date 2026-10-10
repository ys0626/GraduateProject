using UnityEngine;

[System.Serializable]
public class RewardSettings
{
    [Header("Gold")]
    [Min(0)]
    public int minGold;

    [Min(0)]
    public int maxGold;

    [Header("Cheese")]
    [Range(0f, 1f)]
    public float cheeseChance;

    [Min(0)]
    public int cheeseHealAmount;

    [Header("Card")]
    [Min(1)]
    public int cardChoiceCount;
}