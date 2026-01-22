using UnityEngine;
using DG.Tweening;

public class Block : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("Slide Settings")]
    [SerializeField] private float slideDuration = 0.2f;

    public void OnBlocked(Vector2Int blockPos)
    {
        // 움찔거리는 효과
        if (!DOTween.IsTweening(transform))
        {
            transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
                .SetEase(Ease.OutQuad);
        }

        // 킥 이펙트 재생
        EffectManager.Instance.PlayEffectAtGrid(EffectType.Kick, blockPos);
        AudioManager.Instance.PlaySFX(SFXType.BlockKick);
    }

    public void OnSlid(Vector2Int kickedFromPos, Vector2Int targetPos)
    {
        // 킥 이펙트 재생
        EffectManager.Instance.PlayEffectAtGrid(EffectType.Kick, kickedFromPos);
        AudioManager.Instance.PlaySFX(SFXType.BlockKick);
        AudioManager.Instance.PlaySFX(SFXType.BlockMove);

        // 목표 위치로 부드럽게 이동
        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(targetPos);
        transform.DOMove(targetWorldPos, slideDuration)
            .SetEase(Ease.OutExpo);
    }
}
