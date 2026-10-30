using UnityEngine;

public class VulnerableStatus : Debuff
{
    public override StatusType Type =>
        StatusType.Vulnerable;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Vulnerable");

    public override string Description =>
    "공격 카드로 받는 피해량이 25% 증가합니다.";

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