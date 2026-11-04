public enum EventEffectType
{
    None,                   // 효과 없음
    GainGold,               // 골드 획득
    LoseGold,               // 골드 소모
    GainHP,                 // 체력 회복
    LoseHP,                 // 체력 감소
    GainMaxHP,              // 최대 체력 증가
    GainRandomRelic,        // 무작위 유물 획득
    LoseRandomCheese,       // 무작위 치즈 제거
    GainRandomCheese,       // 무작위 치즈 획득
    UpgradeRandomCard       // 무작위 카드 강화
}