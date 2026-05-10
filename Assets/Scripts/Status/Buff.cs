/// <summary>
/// 매 턴 감소하지 않음
/// </summary>
public abstract class Buff : Status
{
    public override bool IsDebuff => false;

    protected Buff(int value)
        : base(value)
    {

    }
}