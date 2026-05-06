using UnityEngine;

/// <summary>
/// 카드 1장의 데이터 정의 (ScriptableObject)
/// CardDatabase에서 참조하여 사용
/// </summary>
[CreateAssetMenu(menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;

    [TextArea]
    public string description;

    public Sprite artwork;

    [Header("Card Info")]
    public int cost;
    public CardType cardType;
    public CardRarity rarity;
    public TargetType targetType;

    [Header("Effect")]
    public CardEffectType cardEffectType;

    // 전투 수치 추가
    public int damage;          // 공격 데미지
    public int blockAmount;     // 방어량
    public int effectAmount;    // 상태이상 수치 (Burn, Weak 등 부여량)
    public int drawAmount;      // 드로우 수
    public int energyAmount;    // 에너지 획득량

    // 특수 옵션
    public bool isExhaust;      // 소멸 카드 여부
    public bool isEthereal;     // 턴 종료 시 소멸 여부
}
