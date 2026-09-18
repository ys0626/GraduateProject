using UnityEngine;


/// 유물의 변하지 않는 원본 데이터
/// Unity 에셋으로 생성하며, 이름·설명·아이콘·효과 종류만 보관
/// 런 도중 바뀌는 충전량이나 획득 순서 등은 RelicInstance에 둔다.

[CreateAssetMenu(menuName = "Relic/Relic Data")]
public class RelicData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string relicId;
    [SerializeField] private string displayName;

    [TextArea]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;
    [SerializeField] private RelicEffectType effectType;

    public string RelicId => relicId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public RelicEffectType EffectType => effectType;
}
