using UnityEngine;

public class EnemyVisualController : MonoBehaviour
{
    [Header("Enemy Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CharacterAnimationController animationController;

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