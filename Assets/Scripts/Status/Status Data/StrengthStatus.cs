using UnityEngine;

public class StrengthStatus : Buff
{
    public override StatusType Type =>
        StatusType.Strength;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Strength");

    public override string Description =>
    "공격 카드의 피해량이 \n힘 수치만큼 증가합니다.";

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