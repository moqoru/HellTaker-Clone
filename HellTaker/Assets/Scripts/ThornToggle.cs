using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornToggle : MonoBehaviour
{
    public Sprite spriteUp;
    public Sprite spriteDown;

    private SpriteRenderer spriteRenderer;
    // private Animator animator;
    private bool isUp;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // animator = GetComponent<Animator>();

        isUp = gameObject.CompareTag("ThornUp");
        UpdateVisual();
    }

    public void Toggle()
    {
        isUp = !isUp;

        gameObject.tag = isUp ? "ThornUp" : "ThornDown";

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // TODO: 프리팹 하나만 쓰고, 올라갔다 내려가는 애니메이션으로 처리하기
        spriteRenderer.sprite = isUp ? spriteUp : spriteDown;
    }
}
