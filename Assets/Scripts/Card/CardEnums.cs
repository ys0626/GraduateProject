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