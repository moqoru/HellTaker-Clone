using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using Unity.VisualScripting;
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

    private string[] romanNumeral = { "O", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI" };
    private int currentMoveCount = 0;
    private bool isStageCleared = false;
    private bool isGameOver = false;
    private bool hasKey = false;
    private GameObject player;

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
    }

    private void Start()
    {
        InitializeStage();
    }

    private void Update()
    {
        /*
        if (isStageCleared || isGameOver)
        {
            return;
        }*/
        // CheckWinCondition();
    }

    /** 맵 초기화 및 UI 반영 */
    private void InitializeStage()
    {
        StartCoroutine(EnableStageCoroutine());
    }

    private void ResetGameState()
    {
        isStageCleared = false;
        isGameOver = false;
        hasKey = false;
        currentMoveCount = 0;

        UpdateUI();
    }
    /** 이동 횟수 미리 차감 방지를 위해 딜레이 */
    IEnumerator EnableStageCoroutine()
    {
        while (InputManager.Instance == null)
        {
            yield return null;
        }

        ResetGameState();
        InputManager.Instance.SetState(GameState.Playing);
        Debug.Log("[GameManager] Initialize 완료");
    }

    private void UpdateUI()
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
    private void CheckWinCondition()
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

        // 이동 횟수 초과 시 게임오버
        // TODO : 게임오버시 캐릭터 사망 애니메이션 재생
        if (currentMoveCount > maxMoveCount)
        {
            OnGameOver();
            return;
        }

        CheckWinCondition();
    }

    private void OnLevelClear()
    {
        isStageCleared = true;
        Debug.Log("=== 레벨 클리어! ===");

        // 대화 콜백 설정
        DialogueManager.Instance.OnDialogueEnd = () =>
        {
            // 정답 선택 시 다음 스테이지로
            StartCoroutine(TransitionToNextStage());
        };

        DialogueManager.Instance.OnWrongChoice = (gameOverMessage) =>
        {
            InputManager.Instance.SetState(GameState.UI, UIType.GameOver);
            DialogueDeathAnimator.Instance.PlayGameOver(gameOverMessage);
        };

        // 대화 시작
        DialogueManager.Instance.StartDialogue(currentStage);
    }

    public void ShowHint()
    {
        // 힌트 종료 시 게임으로 복귀
        DialogueManager.Instance.OnDialogueEnd = () =>
        {
            InputManager.Instance.SetState(GameState.Playing);
        };

        // 인생 조언 시작
        DialogueManager.Instance.StartAdvice(currentStage);
    }

    /** 게임 오버 (이동 횟수 초과) */
    private void OnGameOver()
    {
        isGameOver = true;
        Debug.Log("=== 게임 오버! 이동 횟수 초과 ===");

        // TODO: 캐릭터 죽는 애니메이션 만들고 재생
        // DeathAnimation.Instance.PlayDeath(() => {
        //     InputManager.Instance.SetState(GameState.UI, UIType.GameOver??);
        // });

        // 임시: 바로 재시작
        RestartStage();
    }

    public void RestartStage()
    {
        StartCoroutine(TransitionToRestartStage());
    }

    IEnumerator TransitionToRestartStage()
    {
        InputManager.Instance.SetState(GameState.Transition);

        TransitionAnimator.Instance.PlayTransition();

        // 절반 상태 (화면이 덮인 상태)까지 대기
        yield return new WaitForSeconds(TransitionAnimator.Instance.HalfDuration);

        DialogueDeathAnimator.Instance.HideGameOver();
        // LevelManager에서 맵 리로드
        LevelManager.Instance.ReloadStage();
        ResetGameState();

        // 나머지 절반 (화면이 완전히 보이는 상태)까지 대기
        yield return new WaitForSeconds(TransitionAnimator.Instance.HalfDuration);

        InputManager.Instance.SetState(GameState.Playing);
        Debug.Log("[GameManager] 재시작 완료");
    }

    IEnumerator TransitionToNextStage()
    {
        InputManager.Instance.SetState(GameState.Transition);

        // 트랜지션 애니메이션 실행
        TransitionAnimator.Instance.PlayTransition();

        // 절반 상태 (화면이 덮인 상태)까지 대기
        yield return new WaitForSeconds(TransitionAnimator.Instance.HalfDuration);

        // 스테이지 번호 증가, 맵 로드
        currentStage++;
        LevelManager.Instance.LoadNextStage(currentStage);
        ResetGameState();

        // 나머지 절반 (화면이 완전히 보이는 상태)까지 대기
        yield return new WaitForSeconds(TransitionAnimator.Instance.HalfDuration);

        InputManager.Instance.SetState(GameState.Playing);
        Debug.Log($"[GameManager] {currentStage} 스테이지 맵 로드 완료");
    }

    public int GetRemainingMoves()
    {
        return Mathf.Max(0, maxMoveCount - currentMoveCount);
    }

    public bool IsStageCleared()
    {
        return isStageCleared;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool HasKey()
    {
        return hasKey;
    }

    public void SetKey(bool value)
    {
        hasKey = value;
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
