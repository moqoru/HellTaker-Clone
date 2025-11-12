using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    private Animator animator;
    // 모든 오브젝트의 알파값 조절 대신, 렌더링 레이어로 조절하는 CanvasGroup으로 알파값 조절
    private CanvasGroup canvasGroup;

    void Start()
    {
        animator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 인스펙터에 CanvasGroup이 없을 경우 직접 생성
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        animator.enabled = false;
    }

    public void PlayTransition()
    {
        canvasGroup.alpha = 1f;
        animator.enabled = true;
    }

    public void OnTransitionEnd()
    {
        canvasGroup.alpha = 0f;
        animator.enabled = false;
    }

}
