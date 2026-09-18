using UnityEngine;

/// <summary>
/// 보유 유물 아이콘을 표시하는 UI의 진입점입니다.
/// 실제 아이콘 프리팹 생성·정렬·툴팁 표시는 유물 UI 제작 단계에서 구현합니다.
/// </summary>
public class RelicInventoryUI : MonoBehaviour
{
    [SerializeField] private Transform iconContainer;
    [SerializeField] private GameObject relicIconPrefab;

    /// <summary>유물 획득 또는 런 로드 후 보유 목록을 다시 그립니다.</summary>
    public void Refresh()
    {
        // TODO: RelicManager.Instance.Relics를 읽어 아이콘 UI를 구성한다.
    }
}
