using UnityEngine;

/// <summary>
/// 배틀씬에서 적을 관리하는 클래스
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Reference")]
    private Animator enemyAnimator;

    private void Awake()
    {
        enemyAnimator = GetComponent<Animator>();
    }

    /// <summary>
    /// 적의 공격 애니메이션을 1회 실행합니다
    /// </summary>
    public void playAttackAnimation()
    {
        enemyAnimator.SetTrigger("Attack");
    }
}
