using UnityEngine;

public class WeakStatus : Debuff
{
    public override StatusType Type =>
        StatusType.Weak;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Weak");

    public override string Description =>
    "공격 카드로 주는 피해량이 25% 감소합니다.";

    public WeakStatus(int turns)
        : base(turns)
    {

    }

    /// <summary>
    /// 시뮬레이션에서 사용
    /// </summary>
    /// <returns></returns>
    public override Status Clone()
    {
        return new WeakStatus(Value);
    }
}