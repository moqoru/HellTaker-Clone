using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class DialogueDeathAnimator : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("게임 오버 스프라이트 프레임 목록")]
    public Sprite[] dialogueDeathFrames;
    [Tooltip("프레임당 지속 시간 (초)")]
    public float frameInterval = 0.067f;

    [Header("자기 자신과 자식 오브젝트 할당")]
    public CanvasGroup canvasGroup;
    public Image heartAttackAnimation;
    public TextMeshProUGUI deathMessage;

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    void Start()
    {
        // 인스펙터에 CanvasGroup이 없을 경우 직접 생성
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /** 전환 시간 계산 및 프레임 전환, 종료 처리 */
    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;

        if (timer >= frameInterval)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame < dialogueDeathFrames.Length)
            {
                heartAttackAnimation.sprite = dialogueDeathFrames[currentFrame];
            }
            else
            {
                OnAnimationComplete();
            }
        }
    }

    /** 게임 오버 애니메이션 재생 */
    public void PlayGameOver(string message = "Game Over : Enter를 눌러 재시작")
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        deathMessage.text = message;

        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        if (dialogueDeathFrames.Length > 0)
        {
            heartAttackAnimation.sprite = dialogueDeathFrames[0];
        }
    }

    /** 애니메이션 완료 처리 */
    public void OnAnimationComplete()
    {
        isPlaying = false;
        StartCoroutine(WaitForKeyInput());
    }

    // TODO : InputManager와 연동 필요
    IEnumerator WaitForKeyInput()
    {
        // Debug.Log("스페이스바나 엔터 누를 때까지 대기 중...");
        yield return new WaitUntil(
            () => Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space));

        Debug.Log("키 입력 확인!");

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameManager.Instance.RestartStage();
    }

}
