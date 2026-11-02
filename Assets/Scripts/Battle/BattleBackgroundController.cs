
using UnityEngine;

/// <summary>
/// 전투 종류에 따라 BattleScene의 배경을 변경하는 클래스
/// </summary>
public class BattleBackgroundController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Battle Background Sprites")]
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite eliteBackground;
    [SerializeField] private Sprite bossBackground;

    /// <summary>
    /// 현재 전투 타입에 맞는 배경 설정
    /// </summary>
    public void SetBackground()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[BattleBackgroundController] GameData가 없습니다.",
                this
            );
            return;
        }

        if (backgroundRenderer == null)
        {
            Debug.LogError(
                "[BattleBackgroundController] Background SpriteRenderer가 연결되지 않았습니다.",
                this
            );
            return;
        }

        Sprite selectedBackground = null;

        switch (GameData.instance.CurrentBattleType)
        {
            case BattleType.Normal:
                selectedBackground = normalBackground;
                break;

            case BattleType.Elite:
                selectedBackground = eliteBackground;
                break;

            case BattleType.Boss:
                selectedBackground = bossBackground;
                break;

            default:
                Debug.LogError(
                    "[BattleBackgroundController] 지원하지 않는 전투 타입입니다."
                );
                return;
        }

        if (selectedBackground == null)
        {
            Debug.LogError(
                $"[BattleBackgroundController] " +
                $"{GameData.instance.CurrentBattleType} 배경이 할당되지 않았습니다.",
                this
            );
            return;
        }

        backgroundRenderer.sprite = selectedBackground;

        Debug.Log(
            $"[BattleBackgroundController] 배경 변경 완료: " +
            $"{GameData.instance.CurrentBattleType}"
        );
    }
}
