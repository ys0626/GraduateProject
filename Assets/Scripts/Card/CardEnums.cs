public enum CardType
{
    Attack,   // 공격
    Defense,  // 방어
    Skill,    // 스킬
    Power,    // 파워 (지속 효과)
}

public enum CardRarity
{
    Common,    // 일반
    Uncommon,  // 희귀
    Rare,      // 레어
}

public enum TargetType
{
    Enemy,      // 적 단일
    AllEnemies, // 적 전체
    Self,       // 자신
    None,       // 대상 없음
}

public enum CardEffectType
{
    None,
    Strike,         // 데미지
    Defend,          // 방어
    // 추후 추가
}

