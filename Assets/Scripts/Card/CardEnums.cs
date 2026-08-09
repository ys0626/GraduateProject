/// <summary>
/// 구체적인 카드 종류 ex)타격, 수비, 강타, ...
/// </summary>
public enum CardEffectType
{
    Bash,
    BodySlam,
    Dash,
    Defend,
    Entrench,
    Footwork,
    Inflame,
    Strike,
    SwordBoomerang,
    Taunt,
    TwinStrike,
    Uppercut,
}

public enum CardRarity
{
    Common,
    Uncommon,
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

[System.Flags]
public enum CardTag
{
    None = 0,
    Debuff = 1 << 0, // 상대에게 Weak/Vulnerable 등 부여
    StrengthGain = 1 << 1, // 자신 Strength 증가
    LimitBreak = 1 << 2, // 현재 Strength 배가
    Rampage = 1 << 3, // 연사 - 사용 후 반드시 공격 카드만 허용
}