using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class PlayerDeathAnimator : MonoBehaviour
{
    public static PlayerDeathAnimator Instance { get; private set; }

    [Header("애니메이션 설정")]
    [Tooltip("게임 오버 스프라이트 프레임 목록")]
    public Sprite[] playerDeathFrames;
    [Tooltip("프레임당 지속 시간 (초)")]
    public float frameInterval = 0.04f;

    [Header("UI 요소")]
    public CanvasGroup canvasGroup;
    public Image playerDeathAnimation;

    [Header("위치 보정")]
    [Tooltip("Death 애니메이션 위치 오프셋 (Unity Unit 단위)")]
    public Vector2 worldOffset = new Vector2(0, 3f);

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    public float TotalDuration => playerDeathFrames.Length * frameInterval;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 인스펙터에 CanvasGroup이 없을 경우 직접 생성
        if (canvasGroup == null && !TryGetComponent(out canvasGroup))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;

        if (timer >= frameInterval)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame < playerDeathFrames.Length)
            {
                playerDeathAnimation.sprite = playerDeathFrames[currentFrame];
            }
            else
            {
                OnAnimationComplete();
            }
        }
    }

    public void PlayDeath(Vector3 playerWorldPos)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 월드 좌표에 오프셋 먼저 적용
        Vector3 offsetWorldPos = playerWorldPos + new Vector3(worldOffset.x, worldOffset.y, 0);
        // 월드 좌표를 스크린 좌표로 변환
        Vector2 screenPos = Camera.main.WorldToScreenPoint(offsetWorldPos);

        // RectTransform의 position의 스크린 좌표 할당
        RectTransform rt = playerDeathAnimation.GetComponent<RectTransform>();
        rt.position = screenPos;

        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        // 첫 프레임은 직접 할당
        if (playerDeathFrames.Length > 0)
        {
            playerDeathAnimation.sprite = playerDeathFrames[0];
        }
    }

    private void OnAnimationComplete()
    {
        isPlaying = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

}
