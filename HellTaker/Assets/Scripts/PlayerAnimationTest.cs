using UnityEngine;

public class PlayerAnimationTest : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("DirectionX", horizontal);
    }
}
