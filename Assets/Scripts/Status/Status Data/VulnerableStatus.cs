using UnityEngine;

public class VulnerableStatus : Debuff
{
    public override StatusType Type =>
        StatusType.Vulnerable;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Vulnerable");

    public VulnerableStatus(int turns)
        : base(turns)
    {

    }

    /// <summary>
    /// 시뮬레이션에서 사용
    /// </summary>
    /// <returns></returns>
    public override Status Clone()
    {
        return new VulnerableStatus(Value);
    }
}