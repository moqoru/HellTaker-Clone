using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 설정")]
    [Tooltip("현재 스테이지의 제한 이동 횟수")]
    public int maxMoveCount = 20;

    private int currentMoveCount = 0;
    private bool isLevelCleared = false;
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
        currentMoveCount = 0;
        isLevelCleared = false;
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLevelCleared || isGameOver)
        {
            return;
        }
        CheckWinCondition();
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
        if (isLevelCleared || isGameOver)
        {
            return;
        }

        currentMoveCount += amount;
        Debug.Log($"이동 횟수: {currentMoveCount}");
        // TODO: 이동 횟수 UI 제작

        // 이동 횟수 초과 체크
        if (currentMoveCount >= maxMoveCount)
        {
            OnGameOver();
        }
    }

    /** 까시 페널티 (이동 횟수 +2) */
    public void ApplyThornPenalty()
    {
        IncreaseMoveCount(2);
        Debug.Log("가시를 밟음! 이동 횟수 +2");
    }

    /** 레벨 클리어 */
    void OnLevelClear()
    {
        isLevelCleared = true;
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

    /** 레벨 재시작 */
    public void RestartLevel()
    {
        // TODO: 현재 씬 다시 로드
        Debug.Log("현재 레벨 재시작 (구현 예정");
        /*
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );*/
    }

    /** 다음 레벨 로드 */
    public void LoadNextLevel()
    {
        // TODO: 다음 스테이지 번호 계산해서 로드
        Debug.Log("다음 레벨 로드 (구현 예정)");
    }

    /** 남은 이동 횟수 반환 (UI 표시용) */
    public int GetRemainingMoves()
    {
        return Mathf.Max(0, maxMoveCount - currentMoveCount);
        // TODO: UI에서 이 메서드를 호출할 지, 매니저에서 UI를 호출할 지 결정하기
    }

    /** (디버깅) 레벨 클리어 여부 */
    public bool IsLevelCleared()
    {
        return isLevelCleared;
    }

    /** (디버깅) 게임 오버 여부 */
    public bool IsGameOver()
    {
        return isGameOver;
    }
}
