using UnityEngine;

public class BlockRetentionStatus : Buff
{
    public override StatusType Type =>
        StatusType.BlockRetention;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/BlockRetention");

    public override string Description =>
        "더 이상 턴 시작 시 방어도를 잃지 않습니다.";

    public BlockRetentionStatus()
        : base(1)
    {

    }

    public override Status Clone()
    {
        return new BlockRetentionStatus();
    }
}