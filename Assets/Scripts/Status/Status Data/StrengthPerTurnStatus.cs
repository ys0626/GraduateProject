using UnityEngine;

public class StrengthPerTurnStatus : Buff
{
    public override StatusType Type =>
        StatusType.StrengthPerTurn;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/StrengthPerTurn");

    public override string Description =>
        $"매 턴 시작 시 힘을 {Value} 얻습니다.";

    public StrengthPerTurnStatus(int value)
        : base(value)
    {

    }

    public override Status Clone()
    {
        return new StrengthPerTurnStatus(Value);
    }
}