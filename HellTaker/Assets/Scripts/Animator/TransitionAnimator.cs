using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionAnimator : MonoBehaviour
{
    public static TransitionAnimator Instance { get; private set; }

    [Header("애니메이션 설정")]
    [Tooltip("트랜지션 프레임 목록")]
    public Sprite[] transitionFrames;
    [Tooltip("프레임당 지속 시간 (초")]
    public float frameInterval = 0.04f;

    [Header("오브젝트 할당")]
    public CanvasGroup canvasGroup;
    public Image transitionAnimation;

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    public float TotalDuration => transitionFrames.Length * frameInterval;
    public float HalfDuration => TotalDuration * 0.5f;
    public float OffSFXDuration => TotalDuration * 0.75f;

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

            if (currentFrame < transitionFrames.Length)
            {
                transitionAnimation.sprite = transitionFrames[currentFrame];
            }
            else
            {
                OnTransitionEnd();
            }
        }
    }

    public void PlayTransition()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        // 첫 프레임은 직접 할당
        if (transitionFrames.Length > 0)
        {
            transitionAnimation.sprite = transitionFrames[0];
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.TransitionOn);
            StartCoroutine(PlayTransitionOffSound());
        }
    }

    private IEnumerator PlayTransitionOffSound()
    {
        yield return new WaitForSeconds(OffSFXDuration);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.TransitionOff);
        }
    }

    private void OnTransitionEnd()
    {
        isPlaying = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

}
