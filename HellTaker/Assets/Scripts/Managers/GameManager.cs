using System.Collections;
using System.Collections.Generic;
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

    private string[] romanNumeral = { "O", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
    private int currentMoveCount = 0;
    private bool isStageCleared = false;
    private bool isGameOver = false;
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
        }
    }
    
    void Start()
    {
        InitializeStage();
    }

    void InitializeStage()
    {
        currentMoveCount = 0;
        isStageCleared = false;
        isGameOver = false;

        UpdateUI();
    }

    void Update()
    {
        if (isStageCleared || isGameOver)
        {
            return;
        }
        CheckWinCondition();
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
        if (isStageCleared || isGameOver)
        {
            return;
        }

        currentMoveCount += amount;
        UpdateUI();

        // 이동 횟수 초과 체크
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
        // TODO: 캐릭터와의 대화 씬 전환
    }

    /** 게임 오버 (이동 횟수 초과) */
    void OnGameOver()
    {
        isGameOver = true;
        Debug.Log("=== 게임 오버! 이동 횟수 초과 ===");
        // TODO: 게임 오버 씬 재생 후 재시작 처리
    }

    /** 스테이지 재시작 */
    public void RestartStage()
    {
        // TODO: 현재 씬 다시 로드
        Debug.Log("현재 레벨 재시작 (구현 예정");
        /*
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );*/
    }

    /** 다음 스테이지 로드 */
    public void LoadNextStage()
    {
        currentStage++;
        // TODO: 다음 스테이지 번호 계산해서 로드
        Debug.Log($"다음 스테이지 로드: {currentStage} (구현 예정)");
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
}
