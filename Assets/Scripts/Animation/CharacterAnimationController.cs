
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool isActionPlaying;
    private bool isDead;

    private static readonly int AttackTrigger =
        Animator.StringToHash("Attack");

    private static readonly int PowerTrigger =
        Animator.StringToHash("Power");

    private static readonly int DeathTrigger =
        Animator.StringToHash("Death");


    // =====================================================
    // Unity
    // =====================================================

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError(
                "CharacterAnimationController: " +
                "Animator가 연결되지 않았습니다.",
                this
            );
        }
    }


    // =====================================================
    // Controller 설정
    // =====================================================

    public void SetAnimatorController(
        RuntimeAnimatorController controller)
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError(
                "CharacterAnimationController: " +
                "Animator가 없습니다.",
                this
            );

            return;
        }

        if (controller == null)
        {
            Debug.LogError(
                "CharacterAnimationController: " +
                "RuntimeAnimatorController가 지정되지 않았습니다.",
                this
            );

            return;
        }

        animator.runtimeAnimatorController = controller;

        isActionPlaying = false;
        isDead = false;

        animator.ResetTrigger(AttackTrigger);
        animator.ResetTrigger(PowerTrigger);
        animator.ResetTrigger(DeathTrigger);
    }


    // =====================================================
    // 공격 애니메이션
    // =====================================================

    public void PlayAttackAnimation()
    {
        PlayActionAnimation(AttackTrigger);
    }


    // =====================================================
    // 파워 애니메이션
    // =====================================================

    public void PlayPowerAnimation()
    {
        PlayActionAnimation(PowerTrigger);
    }


    // =====================================================
    // 일반 행동 애니메이션
    // =====================================================

    private void PlayActionAnimation(int trigger)
    {

        if (isDead || isActionPlaying || animator == null)
        {
            return;
        }

        isActionPlaying = true;

        animator.ResetTrigger(AttackTrigger);
        animator.ResetTrigger(PowerTrigger);

        animator.SetTrigger(trigger);
    }


    // =====================================================
    // 사망 애니메이션
    // =====================================================

    public void PlayDeathAnimation()
    {
        if (isDead || animator == null)
        {
            return;
        }

        isDead = true;
        isActionPlaying = false;

        animator.ResetTrigger(AttackTrigger);
        animator.ResetTrigger(PowerTrigger);
        animator.ResetTrigger(DeathTrigger);

        animator.SetTrigger(DeathTrigger);
    }


    // =====================================================
    // Animation Event
    // =====================================================

    // Attack 및 Power 애니메이션의 마지막 프레임에서 호출
    public void OnActionAnimationFinished()
    {
        if (isDead)
        {
            return;
        }

        isActionPlaying = false;
    }


    // =====================================================
    // 상태 확인
    // =====================================================

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsActionPlaying()
    {
        return isActionPlaying;
    }
}
