/// <summary>
/// 엔티티가 가질 수 있는 버프, 디버프들
/// </summary>
public enum StatusType
{
    Dexterity,          //카드의 방어도가 민첩만큼 증가
    Strength,           //카드의 피해가 힘만큼 증가
    Vulnerable,         //카드로 입는 피해가 1.5배
    Weak,               //카드로 입히는 피해가 0.75배
    BlockRetention,     //턴이 시작되어도 방어도를 잃지 않음
    StrengthPerTurn,    //매 턴 시작 시 힘을 얻음
}