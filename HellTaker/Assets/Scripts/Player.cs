using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool readyToMove = true;
    private Vector2Int currentGridPos;

    void Start()
    {
        currentGridPos = GridManager.Instance.WorldToGrid(transform.position);
        GameManager.Instance.SetPlayer(gameObject);

        readyToMove = true;
    }

    void Update()
    {
        /** 게임 오버되거나 클리어 상태, 맵 로드 중일 땐 입력 차단 */
        if (GameManager.Instance.IsGameOver()
            || GameManager.Instance.IsStageCleared())
        {
            Debug.Log("입력 차단됨!");
            return;
        }

        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 입력 '감도' 설정
        if (moveInput.sqrMagnitude > 0.5f)
        {
            if (readyToMove)
            {
                readyToMove = false;

                // 방향 정규화 (대각 입력 방지)
                Vector2Int direction = NormalizeDirection(moveInput);

                TryMove(direction);
            }
        }
        else
        {
            readyToMove = true;
        }
    }

    /** 입력을 4방향 중 하나로 정규화 */
    private Vector2Int NormalizeDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return new Vector2Int(input.x > 0 ? 1 : -1, 0);
        }
        else
        {
            return new Vector2Int(0, input.y > 0 ? 1 : -1);
        }
    }

    /** 이동 시도 */
    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = currentGridPos + direction;

        if (GridManager.Instance.IsPositionBlocked(targetPos))
        {
            return;
        }

        // 밀 수 있는 오브젝트가 밀리는지 체크
        GameObject pushable = GridManager.Instance.GetPushableAt(targetPos);
        if (pushable != null)
        {
            if (!TryPushObject(pushable, targetPos, direction))
            {
                return;
            }
        }

        // 이동 횟수 1회, 가시에 찔릴 경우 이동 횟수 2회
        int moveCount = 1;
        if (GridManager.Instance.IsPositionPunished(targetPos))
        {
            moveCount++;
        }

        // 이동 수행
        GridManager.Instance.MoveObject(gameObject, currentGridPos, targetPos);
        currentGridPos = targetPos;
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
                GameManager.Instance.IncreaseMoveCount(1);
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
}
