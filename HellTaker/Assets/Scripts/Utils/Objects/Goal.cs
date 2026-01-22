using UnityEngine;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class Goal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform loveSign;

    [Header("Animation Settings")]
    [SerializeField] private float floatHeight = 0.1f;
    [SerializeField] private float floatDuration = 0.25f;

    private void Start()
    {
        if (loveSign != null)
        {
            AnimateLoveSign();
        }
    }

    private void AnimateLoveSign()
    {
        Vector3 originalPos = loveSign.localPosition;

        loveSign.DOLocalMoveY(originalPos.y + floatHeight, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        loveSign.DOKill();
    }
}
