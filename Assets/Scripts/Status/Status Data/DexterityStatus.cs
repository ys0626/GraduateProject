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
}