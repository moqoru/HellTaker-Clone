using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Player player;
    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        player = GetComponent<Player>();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("DirectionX", 1);
        }
    }

    private void Update()
    { 
        CheckAnimationState();
    }

    private void CheckAnimationState()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        bool isPlayingAction = stateInfo.IsName("Move")
                            || stateInfo.IsName("Kick");

        if (isPlayingAction && stateInfo.normalizedTime < 1.0f)
        {
            isAnimating = true;
        }
        else
        {
            isAnimating = false;
        }
    }

    public void UpdateDirection(Vector2 direction)
    {
        if (animator != null && direction.x != 0)
        {
            animator.SetFloat("DirectionX", direction.x);
        }
    }

    public void TriggerMove()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[PlayerAnimator] TriggerMove 호출 실패 - Animator가 준비되지 않음");
            return;
        }

        animator.SetTrigger("Move");
        isAnimating = true;
    }

    public void TriggerKick()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[PlayerAnimator] TriggerKick 호출 실패 - Animator가 준비되지 않음");
            return;
        }

        animator.SetTrigger("Kick");
        isAnimating = true;
    }
}