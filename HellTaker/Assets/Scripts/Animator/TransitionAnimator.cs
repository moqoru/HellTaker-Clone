using UnityEngine;
using UnityEngine.UI;

public class TransitionAnimator : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("마지막 제외 프레임들")]
    public Sprite[] mainFrames;
    [Tooltip("마지막 프레임의 좌, 우 부분")]
    public Sprite[] lastFrameSprites;
    [Tooltip("프레임당 지속 시간 (초")]
    public float frameInterval = 0.067f;

    [Header("자기 자신과 자식 오브젝트 할당")]
    public CanvasGroup parentCanvasGroup;
    public CanvasGroup mainCanvasGroup;
    public CanvasGroup[] lastFrameCanvasGroups;
    public Image mainImage;
    public Image[] lastFrameImages;

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    public float TotalDuration => (mainFrames.Length + 2) * frameInterval;

    void Start()
    {
        // 인스펙터에 CanvasGroup이 없을 경우 직접 생성
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = GetComponent<CanvasGroup>();
            if (parentCanvasGroup == null)
            {
                parentCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        DeactiveCanvasGroup(parentCanvasGroup);

        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = mainImage.GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
            {
                mainCanvasGroup = mainImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
        DeactiveCanvasGroup(mainCanvasGroup);

        // 마지막 프레임 이미지는 캔버스 그룹과 이미지 배열 크기가 안 맞으면 조정
        if (lastFrameCanvasGroups == null || lastFrameCanvasGroups.Length != lastFrameImages.Length)
        {
            lastFrameCanvasGroups = new CanvasGroup[lastFrameImages.Length];
        }

        for (int i = 0; i < lastFrameImages.Length; i++)
        {
            if (lastFrameCanvasGroups[i] == null)
            {
                lastFrameCanvasGroups[i] = lastFrameImages[i].GetComponent<CanvasGroup>();
                if (lastFrameCanvasGroups[i] == null)
                {
                    lastFrameCanvasGroups[i] = lastFrameImages[i].gameObject.AddComponent<CanvasGroup>();
                }
            }
            DeactiveCanvasGroup(lastFrameCanvasGroups[i]);
        }

        HideAllImages();
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;

        if (timer >= frameInterval)
        {
            timer = 0f;
            currentFrame++;

            // 일반 프레임 (마지막 전까지)
            if (currentFrame < mainFrames.Length)
            {
                ShowMainFrame(currentFrame);
            }
            // 마지막 프레임
            else if (currentFrame == mainFrames.Length)
            {
                ShowLastFrame();
            }
            else
            {
                OnTransitionEnd();
            }
        }
    }

    public void PlayTransition()
    {
        ActiveCanvasGroup(parentCanvasGroup);
        
        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        // 스프라이트 미할당 오류 방지
        if (mainFrames.Length > 0)
        {
            ShowMainFrame(0);
        }
    }

    void ShowMainFrame(int frameIndex)
    {
        mainImage.sprite = mainFrames[frameIndex];
        ActiveCanvasGroup(mainCanvasGroup);
        
        foreach (CanvasGroup cg in lastFrameCanvasGroups)
        {
            DeactiveCanvasGroup(cg);
        }
    }

    void ShowLastFrame()
    {
        DeactiveCanvasGroup(mainCanvasGroup);
        
        for (int i = 0; i < lastFrameCanvasGroups.Length && i < lastFrameSprites.Length; i++)
        {
            lastFrameImages[i].sprite = lastFrameSprites[i];
            DeactiveCanvasGroup(lastFrameCanvasGroups[i]);
        }
    }

    void ActiveCanvasGroup(CanvasGroup cg)
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void DeactiveCanvasGroup(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    /** Parent를 제외한 이미지 비활성화 */
    void HideAllImages()
    {
        DeactiveCanvasGroup(mainCanvasGroup);
        foreach (CanvasGroup cg in lastFrameCanvasGroups)
        {
            DeactiveCanvasGroup(cg);
        }
    }

    void OnTransitionEnd()
    {
        isPlaying = false;
        DeactiveCanvasGroup(parentCanvasGroup);
        HideAllImages();
    }

}
