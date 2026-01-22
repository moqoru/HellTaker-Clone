using UnityEngine;
using DG.Tweening;

public class MonsterDestroyEffect : MonoBehaviour
{
    [Header("파티클 조각 설정")]
    [SerializeField] private Sprite[] pieceSprites;

    [Header("흩어지기 효과")]
    [SerializeField] private float explosionForce = 4.0f;
    [SerializeField] private float explosionRadius = 3.0f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private int activePieceCount = 0;

    private void Start()
    {
        if (pieceSprites == null || pieceSprites.Length == 0)
        {
            Debug.LogError("[MonsterDestroyEffect] 조각 스프라이트가 할당되지 않았습니다!");
            Destroy(gameObject);
            return;
        }

        AudioManager.Instance.PlaySFX(SFXType.MonsterDestroy);

        CreateAndScatterPieces();

        Destroy(gameObject, duration + fadeOutDuration + 0.5f);
    }

    private void CreateAndScatterPieces()
    {
        activePieceCount = pieceSprites.Length;

        for (int i = 0; i < pieceSprites.Length; i++)
        {
            CreatePiece(i);
        }
    }

    private void CreatePiece(int index)
    {
        // 조각 오브젝트 생성
        GameObject piece = new GameObject($"Piece_{index}");
        piece.transform.SetParent(transform);
        piece.transform.position = transform.position;
        piece.transform.localScale = Vector3.one * 2.0f;

        // 스프라이트 렌더러 추가
        SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
        sr.sprite = pieceSprites[index % pieceSprites.Length];
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 100;

        // 랜덤 방향과 힘 계산
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomForce = Random.Range(explosionForce * 0.5f, explosionForce);
        Vector3 targetPosition = transform.position + (Vector3)(randomDirection * explosionRadius * randomForce);

        // 랜덤 회전 방향
        float randomRotation = Random.Range(-rotationSpeed, rotationSpeed);

        // DOTween 시퀀스로 조각 애니메이션
        Sequence pieceSequence = DOTween.Sequence();

        // 1. 포물선 운동 (위로 올라갔다가 중력에 의해 떨어짐)
        float upwardBoost = Random.Range(0.5f, 1.5f);
        Vector3 midPoint = targetPosition + Vector3.up * upwardBoost;

        // 포물선 경로
        pieceSequence.Append(
            piece.transform.DOPath(
                new Vector3[] { transform.position, midPoint, targetPosition + Vector3.down * gravity * 0.5f },
                duration,
                PathType.CatmullRom
            ).SetEase(Ease.OutQuad)
        );

        // 2. 회전 애니메이션 (동시 재생)
        pieceSequence.Join(
            piece.transform.DORotate(
                new Vector3(0, 0, randomRotation * duration),
                duration,
                RotateMode.FastBeyond360
            ).SetEase(Ease.Linear)
        );

        // 3. 페이드 아웃 (끝부분에서)
        pieceSequence.Append(
            sr.DOFade(0f, fadeOutDuration)
        );

        // 4. 시퀀스 종료 후 조각 파괴
        pieceSequence.OnComplete(() => {
            if (piece != null)
            {
                Destroy(piece);
            }

            activePieceCount--;

            if (activePieceCount <= 0 && gameObject != null)
            {
                Destroy(gameObject);
            }
        });

        // DOTween 자동 정리 설정
        pieceSequence.SetAutoKill(true);
        pieceSequence.SetTarget(piece);
    }
}