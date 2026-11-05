public enum CardRarity
{
    Common,
    Rare
}

public enum CardType
{
    Attack,
    Skill,
    Power
}

public enum TargetType
{
    Self,
    Enemy,
}

public enum CardUpgradeType
{
    EffectUpgrade,
    CostReduction
}

[System.Flags]
public enum CardTag
{
    None = 0,
    Debuff = 1 << 0, // 상대에게 Weak/Vulnerable 등 부여
    StrengthGain = 1 << 1, // 자신 Strength 증가
    LimitBreak = 1 << 2, // 현재 Strength 배가
    DoubleTap = 1 << 3, // 연사 - 사용 후 반드시 공격 카드만 허용
}