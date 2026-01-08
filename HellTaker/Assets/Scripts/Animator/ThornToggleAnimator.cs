using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornToggleAnimator : MonoBehaviour
{
    [Header("애니메이션 프레임")]
    [Tooltip("가시 애니메이션 목록 (올라간 상태 -> 내려간 상태")]
    public Sprite[] thornFrames;
    [Tooltip("프레임당 지속 시간 (초")]
    public float frameInterval = 0.04f;

    private SpriteRenderer spriteRenderer;
    // private Animator animator;
    private bool isUp;
    private bool isAnimating = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        isUp = gameObject.CompareTag("ThornUp");

        if (thornFrames != null && thornFrames.Length > 0)
        {
            spriteRenderer.sprite = thornFrames[isUp ? 0 : thornFrames.Length - 1];
        }
    }

    public void Toggle()
    {
        if (isAnimating) return;

        isUp = !isUp;

        gameObject.tag = isUp ? "ThornUp" : "ThornDown";

        StartCoroutine(PlayAnimation());
    }

    public IEnumerator PlayAnimation()
    {
        isAnimating = true;

        if (thornFrames == null || thornFrames.Length == 0)
        {
            Debug.LogWarning("[ThornToggle] thornFrames가 비어있습니다.");
            isAnimating = false;
            yield break;
        }

        if (!isUp)
        {
            for (int i = 0; i < thornFrames.Length; i++)
            {
                spriteRenderer.sprite = thornFrames[i];
                yield return new WaitForSeconds(frameInterval);
            }
        }
        else
        {
            for (int i = thornFrames.Length - 1; i >= 0; i--)
            {
                spriteRenderer.sprite = thornFrames[i];
                yield return new WaitForSeconds(frameInterval);
            }
        }

        isAnimating = false;
    }
}
