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
        // ¿òÂñ°Å¸®´Â È¿°ú
        if (!DOTween.IsTweening(transform))
        {
            transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
                .SetEase(Ease.OutQuad);
        }

        // Å± ÀÌÆåÆ® Àç»ý
        EffectManager.Instance.PlayEffectAtGrid(EffectType.Kick, blockPos);
        AudioManager.Instance.PlaySFX(SFXType.BlockKick);
    }

}
