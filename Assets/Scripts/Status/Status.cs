using UnityEngine;

public abstract class Status
{
    // 상태 종류
    public abstract StatusType Type { get; }

    // 버프 / 디버프
    public abstract bool IsDebuff { get; }

    // 아이콘
    public abstract Sprite Icon { get; }

    // 수치
    // Buff = 효과 수치
    // Debuff = 지속 턴
    public int Value { get; protected set; }

    protected Status(int value)
    {
        Value = value;
    }

    // 같은 상태 추가 시 처리
    public virtual void AddValue(int amount)
    {
        Value += amount;
    }

    // 턴 종료 시 호출
    public virtual void OnTurnEnd(IBattleEntity owner)
    {

    }

    // 상태 제거 여부
    public virtual bool ShouldRemove()
    {
        return Value <= 0;
    }

    public abstract Status Clone();
}