
using UnityEngine;

public class CardPlayPresentation : MonoBehaviour
{
    [Header("Character Animations")]
    [SerializeField]
    private CharacterAnimationController playerAnimation;

    [Header("Enemy Visual")]
    [SerializeField]
    private EnemyVisualController enemyVisualController;

    private Entity playerEntity;
    private Entity enemyEntity;

    // =====================================================
    // 초기화
    // =====================================================

    public void Initialize(Entity player, Entity enemy)
    {
        playerEntity = player;
        enemyEntity = enemy;

        if (playerAnimation == null)
        {
            Debug.LogError(
                "CardPlayPresentation: " +
                "Player Animation이 연결되지 않았습니다.",
                this
            );
        }

        if (enemyVisualController == null)
        {
            Debug.LogError(
                "CardPlayPresentation: " +
                "EnemyVisualController가 연결되지 않았습니다.",
                this
            );
        }
    }

    // =====================================================
    // 적 시각 정보 설정
    // =====================================================

    public void SetEnemyVisual(EnemyData enemyData)
    {
        if (enemyVisualController == null)
        {
            Debug.LogError(
                "CardPlayPresentation: " +
                "EnemyVisualController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        enemyVisualController.SetEnemyVisual(enemyData);
    }

    // =====================================================
    // 적 Animator Controller 설정
    // =====================================================

    public void SetEnemyAnimatorController(
        RuntimeAnimatorController controller)
    {
        if (enemyVisualController == null)
        {
            Debug.LogError(
                "CardPlayPresentation: " +
                "EnemyVisualController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        enemyVisualController.SetEnemyAnimatorController(controller);
    }

    // =====================================================
    // 카드 사용 애니메이션
    // =====================================================

    public void PlayCardAnimation(
        Entity user,
        CardType cardType)
    {
        CharacterAnimationController animationController =
            GetAnimationController(user);

        if (animationController == null)
        {
            return;
        }

        switch (cardType)
        {
            case CardType.Attack:
                animationController.PlayAttackAnimation();
                break;

            case CardType.Power:
                animationController.PlayPowerAnimation();
                break;
        }
    }

    // =====================================================
    // 사망 애니메이션
    // =====================================================

    public void PlayDeathAnimation(Entity deadEntity)
    {
        CharacterAnimationController animationController =
            GetAnimationController(deadEntity);

        if (animationController == null)
        {
            return;
        }

        animationController.PlayDeathAnimation();
    }

    // =====================================================
    // Entity에 해당하는 애니메이션 컨트롤러 반환
    // =====================================================

    private CharacterAnimationController GetAnimationController(
        Entity entity)
    {
        if (entity == null)
        {
            return null;
        }

        if (entity == playerEntity)
        {
            return playerAnimation;
        }

        if (entity == enemyEntity)
        {
            if (enemyVisualController == null)
            {
                return null;
            }

            return enemyVisualController.GetAnimationController();
        }

        return null;
    }
}
