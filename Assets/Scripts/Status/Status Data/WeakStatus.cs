using UnityEngine;

public class WeakStatus : Debuff
{
    public override StatusType Type =>
        StatusType.Weak;

    public override Sprite Icon =>
        Resources.Load<Sprite>(
            "StatusIcons/Weak");

    public WeakStatus(int turns)
        : base(turns)
    {

    }
}