using UnityEngine;

public class PlayerAnimationTest : MonoBehaviour
{
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("DirectionX", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            animator.SetFloat("DirectionX", -1);
            animator.SetTrigger("Move");
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            animator.SetFloat("DirectionX", 1);
            animator.SetTrigger("Move");
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            animator.SetTrigger("Move");
        }
    }
}
