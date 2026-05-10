using UnityEngine;

public class StrengthStatus : Buff
{
    public override StatusType Type =>
        StatusType.Strength;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Strength");

    public StrengthStatus(int value)
        : base(value)
    {

    }
}