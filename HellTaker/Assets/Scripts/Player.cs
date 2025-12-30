using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance {  get; private set; }

    [HideInInspector] public Vector2Int lastMoveDirection = Vector2Int.right;
    private Vector2Int currentGridPos;

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
        currentGridPos = GridManager.Instance.WorldToGrid(transform.position);
        GameManager.Instance.SetPlayer(gameObject);
    }

    private void Update()
    {
        // 입력 처리 부분을 InputManager로 이동
    }

    private void OnDestroy()
    {
        // Player가 파괴될 때 Instance를 null로 초기화, 새로 만들도록 하기
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /** 이동 시도 */
    public void TryMove(Vector2Int direction)
    {
        lastMoveDirection = direction;

        // Playing 상태일 때만 이동 가능
        if (InputManager.Instance.GetState() != GameState.Playing)
        {
            Debug.LogError("[Player] Playing 상태가 아닐 때 이동을 시도했습니다.");
            return;
        }

        // 게임오버나 클리어 상태면 이동 불가
        if (GameManager.Instance.IsGameOver() || GameManager.Instance.IsStageCleared())
        {
            return;
        }

        Vector2Int targetPos = currentGridPos + direction;

        // 자물쇠 체크 (열쇠를 갖고 있다면 이동 전 '미리' 자물쇠 제거)
        GameObject lockBox = GridManager.Instance.GetObjectWithTagAt(targetPos, "LockBox");
        if (lockBox != null)
        {
            if (GameManager.Instance.HasKey())
            {
                UnlockBox(lockBox, targetPos);
            }
            else
            {
                return;
            }
        }

        // 이동 불가능하면 이동 막기
        if (GridManager.Instance.IsPositionBlocked(targetPos))
        {
            return;
        }

        GameObject pushable = GridManager.Instance.GetPushableAt(targetPos);

        if (pushable != null)
        {
            // 밀 수 있는 게 있으면 제자리에서 그 물체만 걷어 차기
            // TODO: 블럭을 걷어 찰 경우 걷어차는 모션 재생 필요, 블럭이 움직일 때와 안 움직일 때 모션 구분

            // 킥 애니메이션 트리거
            // TODO: PlayerAnimator 추가 후 활성화
            // GetComponent<PlayerAnimator>()?.TriggerKick();

            TryPushObject(pushable, targetPos, direction);
        }
        else
        {
            // 이동 애니메이션 트리거
            // TODO: PlayerAnimator 추가 후 활성화
            // GetComponent<PlayerAnimator>()?.TriggerMove();

            // 밀 수 있는 게 없으면 일반 이동 수행
            GridManager.Instance.MoveObject(gameObject, currentGridPos, targetPos);
            currentGridPos = targetPos;

            // 열쇠 획득 처리
            GameObject key = GridManager.Instance.GetObjectWithTagAt(currentGridPos, "Key");
            if (key != null)
            {
                CollectKey(key);
            }
        }

        GridManager.Instance.ToggleThorns();

        int moveCount = 1;
        // 이동한 지점에서 가시에 찔릴 경우 이동 횟수 2회 처리
        if (GridManager.Instance.IsPositionPunished(currentGridPos))
        {
            moveCount++;
        }
        
        GameManager.Instance.IncreaseMoveCount(moveCount);
    }

    /** 오브젝트 밀기 시도 */
    private bool TryPushObject(GameObject pushable, Vector2Int pushablePos, Vector2Int direction)
    {
        Vector2Int pushTargetPos = pushablePos + direction;

        // 밀릴 위치가 장애물이나 다른 밀리는 오브젝트로 막혀있는지 체크
        if (GridManager.Instance.IsPositionBlocked(pushTargetPos)
            || GridManager.Instance.GetPushableAt(pushTargetPos))
        {
            // Monster는 벽에 밀리면 제거 처리
            if (pushable.CompareTag("Monster"))
            {
                DestroyMonster(pushable, pushablePos);
                return false;
            }

            return false; // Block은 벽으로 밀 수 없음
        }

        // 밀릴 위치에 다른 밀 수 있는 오브젝트 있는지 체크
        GameObject anotherPushable = GridManager.Instance.GetPushableAt(pushTargetPos);
        if (anotherPushable != null)
        {
            return false; // Block이 연속으로 있으면 밀 수 없음
        }

        // 밀기 처리
        GridManager.Instance.MoveObject(pushable, pushablePos, pushTargetPos);

        return true;
    }

    /** 몬스터 제거 */
    private void DestroyMonster(GameObject monster, Vector2Int monsterPos)
    {
        GridManager.Instance.UnregisterObject(monster);
        Destroy(monster);
    }

    /** 열쇠 획득 처리 */
    private void CollectKey(GameObject key)
    {
        GameManager.Instance.SetKey(true);
        GridManager.Instance.UnregisterObject(key);
        Destroy(key);
        Debug.Log("열쇠 획득");
    }

    /** 자물쇠 열기 처리 */
    private void UnlockBox(GameObject lockBox, Vector2Int lockBoxPos)
    {
        GridManager.Instance.UnregisterObject(lockBox);
        Destroy(lockBox);
        Debug.Log("자물쇠 해제");
    }
}
