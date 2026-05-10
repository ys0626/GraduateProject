/// <summary>
/// 엔티티가 가질 수 있는 버프, 디버프들
/// </summary>
public enum StatusType
{
    Dexterity,      //카드의 방어도가 민첩만큼 증가
    Strength,       //카드의 데미지가 힘만큼 증가
    Vulnerable,     //데미지로 입는 피해가 1.5배
    Weak,           //데미지로 입히는 피해가 0.75배
}