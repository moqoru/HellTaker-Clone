using UnityEngine;
using DG.Tweening;

public class LockBox : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private int shakeVibrato = 10;

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

}
