using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 설정")]
    [Tooltip("현재 스테이지의 제한 이동 횟수")]
    public int maxMoveCount = 23;
    [Tooltip("현재 스테이지 번호")]
    public int currentStage = 1;

    [Header("UI 참조")]
    [Tooltip("이동 가능 횟수 텍스트")]
    public TextMeshProUGUI turnText;
    [Tooltip("스테이지 텍스트")]
    public TextMeshProUGUI stageText;

    [Header("애니메이션 참조")]
    [SerializeField] private TransitionAnimator transitionAnimator;
    [SerializeField] private DialogueDeathAnimator dialogueDeathAnimator;

    private string[] romanNumeral = { "O", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
    private int currentMoveCount = 0;
    private bool isStageCleared = false;
    private bool isGameOver = true;
    private bool hasKey = false;
    private GameObject player;

    void Awake()
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
    }
    
    void Start()
    {
        InitializeStage();
    }

    void Update()
    {
        if (isStageCleared || isGameOver)
        {
            return;
        }
        // CheckWinCondition();
    }

    /** 맵 초기화 및 UI 반영 */
    void InitializeStage()
    {
        StartCoroutine(EnableStageAfterFrames(1));
        
        isStageCleared = false;
        isGameOver = false;
        hasKey = false;

    }

    /** 이동 횟수 미리 차감 방지를 위해 딜레이 */
    IEnumerator EnableStageAfterFrames(int frames = 1)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }
        currentMoveCount = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (turnText != null)
        {
            int remainingMoves = maxMoveCount - currentMoveCount;
            if (remainingMoves <= 0)
            {
                turnText.text = "X";
            }
            else
            {
                turnText.text = remainingMoves.ToString();
            }
        }
        else
        {
            Debug.LogWarning("TurnText가 할당되지 않았습니다!");
        }

        if (stageText != null)
        {
            if (currentStage >= 0 && currentStage < romanNumeral.Length)
            {
                stageText.text = romanNumeral[currentStage];
            }
            else
            {
                stageText.text = "?";
            }
        }
        else
        {
            Debug.LogWarning("StageText가 할당되지 않았습니다!");
        }
    }

    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
    }

    /** 클리어 조건 체크 (플레이어 상하좌우에 Goal 있는 지)*/
    void CheckWinCondition()
    {
        if (player == null)
        {
            return;
        }

        Vector2Int playerPos = GridManager.Instance.WorldToGrid(player.transform.position);

        Vector2Int[] adjacentDirs = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int dir in adjacentDirs)
        {
            Vector2Int checkPos = playerPos + dir;
            GameObject goal = GridManager.Instance.GetObjectWithTagAt(checkPos, "Goal");

            if (goal != null)
            {
                OnLevelClear();
                return;
            }
        }
    }

    /** 이동 횟수 증가 (일반 이동 처리) */
    public void IncreaseMoveCount(int amount = 1)
    {
        // TODO: 가시에 닿아 2번 이동 페널티 처리 시 UI와 이펙트로 데미지 처리
        if (isStageCleared || isGameOver)
        {
            return;
        }

        currentMoveCount += amount;

        UpdateUI();

        CheckWinCondition();

        // 게임 미 클리어시, 이동 횟수 초과 체크
        if (isStageCleared) return;

        if (currentMoveCount >= maxMoveCount)
        {
            OnGameOver();
        }
    }

    /** 레벨 클리어 */
    void OnLevelClear()
    {
        isStageCleared = true;
        Debug.Log("=== 레벨 클리어! ===");
        
        // TODO: 캐릭터와의 대화 시스템 구현 후 활성화
        // InputManager.Instance.SetState(GameState.UI, UIType.Dialogue);
        // DialogueManager.Instacne.StartDialogue(currentStage);

        // 임시 : 바로 다음 스테이지 로드
        StartCoroutine(LoadNextStageAfterFrames(1));
    }

    /** 게임 오버 (이동 횟수 초과) */
    void OnGameOver()
    {
        isGameOver = true;
        Debug.Log("=== 게임 오버! 이동 횟수 초과 ===");

        InputManager.Instance.SetState(GameState.UI, UIType.GameOver);

        // TODO: 게임 오버 씬 테스트용. 추후 대화 선택지에서만 나오게 변경 후 여기는 트랜지션만 재생
        if (dialogueDeathAnimator != null)
        {
            dialogueDeathAnimator.PlayGameOver();
        }
        else
        {
            RestartStage();
        }
    }

    /** 스테이지 재시작 */
    public void RestartStage()
    {
        // 트랜지션 애니메이션 실행
        transitionAnimator.PlayTransition();

        // GameManager 상태 초기화
        InitializeStage();

        // LevelManager에서 맵 리로드
        LevelManager.Instance.ReloadStage();
    }

    /** 게임 오버와 꼬이지 않게 다음 스테이지 로드 */
    IEnumerator LoadNextStageAfterFrames(int frames = 1)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }

        // 트랜지션 애니메이션 실행
        transitionAnimator.PlayTransition();
        // TODO: '완전히 화면이 덮였을 때' 다음 맵 로드하기

        // 스테이지 번호 증가
        currentStage++;

        // GameManager 상태 초기화
        InitializeStage();

        // LevelManager에서 다음 맵 로드
        LevelManager.Instance.LoadNextStage(currentStage);
        Debug.Log($"[LoadNextStage] {currentStage} 스테이지 맵 로드 완료");
    }

    /** 남은 이동 횟수 반환 */
    public int GetRemainingMoves()
    {
        return Mathf.Max(0, maxMoveCount - currentMoveCount);
    }

    /** 스테이지 클리어 여부 */
    public bool IsStageCleared()
    {
        return isStageCleared;
    }

    /** 게임 오버 여부 */
    public bool IsGameOver()
    {
        return isGameOver;
    }

    /** 키 획득 여부 확인 */
    public bool HasKey()
    {
        return hasKey;
    }

    /** 키 획득 여부 변경 */
    public void SetKey(bool value)
    {
        hasKey = value;
    }
}
