using UnityEngine;

public class DexterityStatus : Buff
{
    public override StatusType Type =>
        StatusType.Dexterity;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Dexterity");

    public override string Description =>
    "카드로 획득하는 방어도가 민첩 수치만큼 증가합니다.";

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