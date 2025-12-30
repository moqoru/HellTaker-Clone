using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Player player;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        animator.SetFloat("DirectionX", 1);
    }

    private void Update()
    {
        float directionX = player.lastMoveDirection.x;
        animator.SetFloat("DirectionX", directionX);
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
