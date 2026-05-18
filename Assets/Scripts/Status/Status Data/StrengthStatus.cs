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

    /// <summary>
    /// 시뮬레이션에서 사용
    /// </summary>
    /// <returns></returns>
    public override Status Clone()
    {
        return new StrengthStatus(Value);
    }
}