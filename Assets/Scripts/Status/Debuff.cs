/// <summary>
/// 매 턴 감소
/// </summary>
public abstract class Debuff : Status
{
    public override bool IsDebuff => true;

    protected Debuff(int value)
        : base(value)
    {

    }

    public override void OnTurnEnd(IBattleEntity owner)
    {
        Value--;
    }
}