using UnityEngine;

/// <summary>
/// 배틀씬에서 플레이어 캐릭터를 관리하는 클래스
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Reference")]
    private Animator playerAnimator;

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
    }

    /// <summary>
    /// 플레이어의 공격 애니메이션을 1회 실행합니다
    /// </summary>
    public void playAttackAnimation()
    {
        playerAnimator.SetTrigger("Attack");
    }
}
