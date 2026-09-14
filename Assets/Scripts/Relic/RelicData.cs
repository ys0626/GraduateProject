using UnityEngine;

/// <summary>
/// 유물 데이터
/// 유물 파트 임시 스텁 - 실제 필드/효과는 추후 교체
/// (상점 파트가 컴파일 가능하도록 최소한의 형태만 먼저 정의함)
/// </summary>
[CreateAssetMenu(menuName = "Relic/RelicData")]
public class RelicData : ScriptableObject
{
    [Header("Basic Info")]
    public string relicName;

    [TextArea]
    public string description;

    public Sprite icon;
}
