using UnityEngine;

public enum CheeseType
{
    SmallNormal,
    LargeNormal,
    SmallRotten,
    LargeRotten
}

[CreateAssetMenu(
    fileName = "NewCheese",
    menuName = "Game/Cheese Data"
)]
public class CheeseData : ScriptableObject
{
    [Header("Basic")]
    public string cheeseName;
    public CheeseType cheeseType;
    public Sprite icon;

    [Header("Effect")]
    [Min(0)]
    public int effectAmount = 10;

    public bool IsRotten
    {
        get
        {
            return cheeseType == CheeseType.SmallRotten ||
                   cheeseType == CheeseType.LargeRotten;
        }
    }

    public string GetDescription()
    {
        if (IsRotten)
        {
            return "최대 HP +" + effectAmount +
                   ", 현재 HP +" + effectAmount;
        }

        return "현재 HP +" + effectAmount;
    }
}