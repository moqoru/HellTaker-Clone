using UnityEngine;

public class MonsterAnimator : MonoBehaviour
{
    private Animator animator;
    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("[MonsterAnimator] Animator 컴포넌트를 찾을 수 없습니다!");
        }
    }

    private void Start()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // 기본 방향: 왼쪽(-1)
            animator.SetFloat("DirectionX", -1);
        }
        else
        {
            Debug.LogError("[MonsterAnimator] Animator Controller가 할당되지 않았습니다!");
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

        bool isPlayingAction = stateInfo.IsName("Hit");

        if (isPlayingAction && stateInfo.normalizedTime < 1.0f)
        {
            isAnimating = true;
        }
        else
        {
            isAnimating = false;
        }
    }

    /** 몬스터가 밀릴 때 밀린 방향의 반대쪽을 바라보도록 설정, Hit 애니메이션 재생*/
    public void OnPushed(Vector2Int pushDirection)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        if (pushDirection.x != 0)
        {
            float facingDirection = -pushDirection.x;
            animator.SetFloat("DirectionX", facingDirection);
        }

        animator.SetTrigger("Hit");
        isAnimating = true;

        AudioManager.Instance.PlaySFX(SFXType.MonsterMove);
    }
}
