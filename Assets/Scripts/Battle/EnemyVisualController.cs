
using UnityEngine;

public class EnemyVisualController : MonoBehaviour
{
    [Header("Enemy Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CharacterAnimationController animationController;
    [SerializeField] private Transform spriteVisual;

    [Header("Normal Layout")]
    [SerializeField]
    private Vector3 normalPosition =
        new Vector3(2.79f, 0.54f, 0f);

    [SerializeField]
    private Vector3 normalScale =
        new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Elite Layout")]
    [SerializeField]
    private Vector3 elitePosition =
        new Vector3(2.79f, 0.54f, 0f);

    [Header("Crystal Falconer Elite Layout")]
    [SerializeField]
    private Vector3 crystalFalconerElitePosition =
    new Vector3(2.94f, 1.19f, 0f);

    [SerializeField]
    private Vector3 eliteScale =
        new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Boss Layout")]
    [SerializeField]
    private Vector3 bossPosition =
        new Vector3(2.79f, 0.54f, 0f);

    [SerializeField]
    private Vector3 bossScale =
        new Vector3(0.9f, 0.9f, 0.9f);


    // =====================================================
    // 적 시각 정보 설정
    // =====================================================

    public void SetEnemyVisual(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError(
                "[EnemyVisualController] EnemyData가 없습니다.",
                this
            );

            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError(
                "[EnemyVisualController] SpriteRenderer가 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (enemyData.sprite == null)
        {
            Debug.LogError(
                $"[EnemyVisualController] {enemyData.enemyName}의 Sprite가 없습니다.",
                this
            );
        }
        else
        {
            spriteRenderer.sprite = enemyData.sprite;
        }

        // 전투 유형에 따른 적 배치 적용
        ApplyBattleLayout(enemyData);

        if (animationController != null)
        {
            animationController.SetAnimatorController(
                enemyData.animatorController
            );
        }
        else
        {
            Debug.LogWarning(
                "[EnemyVisualController] CharacterAnimationController가 연결되지 않았습니다.",
                this
            );
        }

        Debug.Log(
            $"[EnemyVisualController] 적 시각 정보 설정 완료: {enemyData.enemyName}"
        );
    }


    // =====================================================
    // Battle Layout
    // =====================================================

    private void ApplyBattleLayout(EnemyData enemyData)
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[EnemyVisualController] GameData가 없습니다.",
                this
            );
            return;
        }

        // Crystal Falconer 전용 배치
        if (GameData.instance.CurrentBattleType == BattleType.Elite &&
            enemyData.enemyName == "Crystal Falconer")
        {
            transform.localPosition = crystalFalconerElitePosition;
            transform.localScale = eliteScale;
            return;
        }

        // 일반적인 전투 유형별 배치
        switch (GameData.instance.CurrentBattleType)
        {
            case BattleType.Normal:
                transform.localPosition = normalPosition;
                transform.localScale = normalScale;
                break;

            case BattleType.Elite:
                transform.localPosition = elitePosition;
                transform.localScale = eliteScale;
                break;

            case BattleType.Boss:
                transform.localPosition = bossPosition;
                transform.localScale = bossScale;
                break;
        }
    }


    // =====================================================
    // Animation Controller
    // =====================================================

    public CharacterAnimationController GetAnimationController()
    {
        return animationController;
    }


    public void SetEnemyAnimatorController(
        RuntimeAnimatorController controller)
    {
        if (animationController == null)
        {
            Debug.LogError(
                "[EnemyVisualController] " +
                "CharacterAnimationController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        animationController.SetAnimatorController(controller);
    }
}
