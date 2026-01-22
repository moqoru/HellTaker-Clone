using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 설정")]
    [Tooltip("현재 스테이지의 제한 이동 횟수")]
    public int maxMoveCount = 23;
    [Tooltip("현재 스테이지 번호")]
    public int currentStage = 1;

    [Header("UI 참조")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI keyGuideText;

    [Header("이동 횟수 이펙트")]
    [Tooltip("글자 크기 펀치 강도")]
    public float movePunchScale = 0.3f;
    [Tooltip("이펙트 지속 시간")]
    public float moveEffectDuration = 0.2f;
    [Tooltip("페널티 색상")]
    public Color penaltyColor = Color.red;

    private string[] romanNumeral = { "O", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI" };
    
    private int currentMoveCount = 0;
    private bool isStageCleared = false;
    private bool isGameOver = false;
    private bool isPendingGameOver = false;
    private bool hasKey = false;
    private GameObject player;
    private Color originalTurnTextColor;
    private Vector3 originalTurnTextScale;

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
        if (turnText != null)
        {
            originalTurnTextColor = turnText.color;
            originalTurnTextScale = turnText.transform.localScale;
        }
        else
        {
            Debug.LogWarning("[GameManager] TurnText가 할당되지 않았습니다!");
        }

        if (AudioManager.Instance != null && AudioManager.Instance.gameBGM != null)
        {
            AudioManager.Instance.PlayBGM(BGMType.Game);
        }
        InitializeStage();
    }

    private void InitializeStage()
    {
        StartCoroutine(EnableStageCoroutine());
    }

    private void ResetGameState()
    {
        isStageCleared = false;
        isGameOver = false;
        isPendingGameOver = false;
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
            Debug.LogWarning("[GameManager] TurnText가 할당되지 않았습니다!");
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
            Debug.LogWarning("[GameManager] StageText가 할당되지 않았습니다!");
        }
    }

    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
    }

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

    public void IncreaseMoveCount(int amount = 1)
    {
        
        if (isStageCleared || isGameOver)
        {
            return;
        }

        currentMoveCount += amount;
        UpdateUI();

        PlayMoveEffect(amount);

        // 이동 횟수 초과 시 게임오버 대기 처리
        if (currentMoveCount >= maxMoveCount && !isPendingGameOver)
        {
            isPendingGameOver = true;
        }

        CheckWinCondition();
    }

    private void PlayMoveEffect(int amount)
    {
        if (turnText == null) return;

        turnText.transform.DOKill();
        turnText.DOKill();

        turnText.transform.localScale = originalTurnTextScale;
        turnText.color = originalTurnTextColor;

        turnText.transform.DOPunchScale(
            Vector3.one * movePunchScale,
            moveEffectDuration,
            vibrato: 3,
            elasticity: 0.5f
        );

        if (amount >= 2)
        {
            Sequence colorSequence = DOTween.Sequence();
            colorSequence.Append(turnText.DOColor(penaltyColor, moveEffectDuration * 0.5f));
            colorSequence.Append(turnText.DOColor(originalTurnTextColor, moveEffectDuration * 0.5f));
        }

    }

    /** 스테이지 스킵 (TODO: 이후 일시정지 메뉴로 넣기) */
    public void SkipToDialogue()
    {
        if (isStageCleared || isGameOver)
        {
            return;
        }

        OnLevelClear();
    }

    private void OnLevelClear()
    {
        isStageCleared = true;

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

    private void OnGameOver()
    {
        isGameOver = true;

        InputManager.Instance.SetState(GameState.Transition);

        StartCoroutine(PlayDeathAndRestart());
    }

    public void RestartStage()
    {
        StartCoroutine(TransitionToRestartStage());
    }

    IEnumerator PlayDeathAndRestart()
    {
        if (player != null)
        {
            PlayerDeathAnimator.Instance.PlayDeath(player.transform.position);

            yield return new WaitForSeconds(PlayerDeathAnimator.Instance.TotalDuration);
        }

        RestartStage();
    }

    IEnumerator TransitionToRestartStage()
    {
        InputManager.Instance.SetState(GameState.Transition);

        TransitionAnimator.Instance.PlayTransition();

        // 절반 상태 (화면이 덮인 상태)까지 대기
        yield return new WaitForSeconds(TransitionAnimator.Instance.HalfDuration);

        DialogueDeathAnimator.Instance.HideGameOver();

        if (player != null)
        {
            if (player.TryGetComponent<PlayerAnimator>(out PlayerAnimator playerAnimator))
            {
                //playerAnimator.ResetDeath();
            }
        }

        LevelManager.Instance.ReloadStage();
        ResetGameState();

        // 나머지 절반 (화면이 완전히 보이는 상태)까지 대기
        yield return new WaitForSeconds(TransitionAnimator.Instance.HalfDuration);

        InputManager.Instance.SetState(GameState.Playing);
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

    public bool IsPendingGameOver()
    {
        return isPendingGameOver;
    }

    public void ExecutePendingGameOver()
    {
        if (isPendingGameOver)
        {
            isPendingGameOver = false;
            OnGameOver();
        }
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
