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
}