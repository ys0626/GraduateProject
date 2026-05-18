using UnityEngine;

public class DexterityStatus : Buff
{
    public override StatusType Type =>
        StatusType.Dexterity;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Dexterity");

    public DexterityStatus(int value)
        : base(value)
    {

    }

    /// <summary>
    /// 시뮬레이션에서 사용
    /// </summary>
    /// <returns></returns>
    public override Status Clone()
    {
        return new DexterityStatus(Value);
    }
}