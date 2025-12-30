using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Player player;

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
        if (player.lastMoveDirection.x != 0)
        {
            animator.SetFloat("DirectionX", player.lastMoveDirection.x);
        }
    }

    public void TriggerMove()
    {
        animator.SetTrigger("Move");
    }

    public void TriggerKick()
    {
        animator.SetTrigger("Kick");
    }
}