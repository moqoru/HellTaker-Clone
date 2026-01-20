using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [HideInInspector] public Vector2Int lastMoveDirection = Vector2Int.right;
    private PlayerAnimator playerAnimator;
    private Vector2Int currentGridPos;

    // 딜레이 입력 처리 (큐 활용)
    private Vector2Int? queuedInput = null;

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

        playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void Start()
    {
        currentGridPos = GridManager.Instance.WorldToGrid(transform.position);
        GameManager.Instance.SetPlayer(gameObject);
    }

    private void Update()
    {
        // 게임오버 대기 중엔 큐 입력도 막기
        if (GameManager.Instance.IsPendingGameOver())
        {
            queuedInput = null;
            return;
        }

        // 애니메이션이 끝났고 큐에 입력이 남아 있으면 마저 실행
        if (playerAnimator != null && !playerAnimator.IsAnimating && queuedInput.HasValue)
        {
            Vector2Int input = queuedInput.Value;
            queuedInput = null;
            TryMove(input);
        }
    }

    private void OnDestroy()
    {
        // Player가 파괴될 때 Instance를 null로 초기화, 새로 만들도록 하기
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void TryMove(Vector2Int direction)
    { 
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

        // 애니메이션 재생 중이면 입력을 큐에 저장
        if (playerAnimator != null && playerAnimator.IsAnimating)
        {
            queuedInput = direction;
            return;
        }

        lastMoveDirection = direction;

        Vector2Int targetPos = currentGridPos + direction;

        // 이동 불가능하면 이동 막기
        if (GridManager.Instance.IsPositionBlocked(targetPos))
        {
            return;
        }

        // 게임오버 대기 상태라면 이동하지 않고 게임오버 처리
        if (GameManager.Instance.IsPendingGameOver())
        {
            queuedInput = null;
            GameManager.Instance.ExecutePendingGameOver();
            return;
        }

        if (playerAnimator != null)
        {
            playerAnimator.UpdateDirection(direction);
        }

        GameObject pushable = GridManager.Instance.GetPushableAt(targetPos);

        if (pushable != null)
        {
            // 자물쇠를 열쇠로 열 경우만 특별 처리
            if (pushable.CompareTag("LockBox") && GameManager.Instance.HasKey())
            {
                if (playerAnimator != null)
                {
                    playerAnimator.TriggerMove();
                }

                UnlockBox(pushable);

                // 자물쇠 있던 자리로 이동
                GridManager.Instance.MoveObject(gameObject, currentGridPos, targetPos);
                EffectManager.Instance.PlayEffectAtGrid(EffectType.Move, currentGridPos);
                currentGridPos = targetPos;
            }
            else
            {
                // 블록/몬스터/열쇠 없는 자물쇠 → 킥 애니메이션 + 제자리
                if (playerAnimator != null)
                {
                    playerAnimator.TriggerKick();
                }

                TryPushObject(pushable, targetPos, direction);
            }
        }
        else
        {
            if (playerAnimator != null)
            {
                playerAnimator.TriggerMove();
            }

            // 밀 수 있는 게 없으면 일반 이동 수행
            GridManager.Instance.MoveObject(gameObject, currentGridPos, targetPos);
            EffectManager.Instance.PlayEffectAtGrid(EffectType.Move, currentGridPos);
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
            EffectManager.Instance.PlayEffectAtGrid(EffectType.Damage, currentGridPos);

            if (playerAnimator != null)
            {
                playerAnimator.FlashDamage();
            }
        }
        
        GameManager.Instance.IncreaseMoveCount(moveCount);
    }

    private bool TryPushObject(GameObject pushable, Vector2Int pushablePos, Vector2Int direction)
    {
        Vector2Int pushTargetPos = pushablePos + direction;
        Block block;

        // LockBox인데 열쇠가 없을 경우
        if (pushable.CompareTag("LockBox"))
        {
            if (pushable.TryGetComponent(out LockBox lockBox))
            {
                lockBox.OnBlocked(pushablePos);
            }
            return false;
        }

        // 밀릴 위치가 장애물이나 다른 밀리는 오브젝트로 막혀있는지 체크
        if (GridManager.Instance.IsPositionBlocked(pushTargetPos)
            || GridManager.Instance.GetPushableAt(pushTargetPos))
        {
            // Monster는 벽에 밀리면 제거 처리
            if (pushable.CompareTag("Monster"))
            {
                // 밀린 방향에 따라 몬스터 방향 전환 및 Hit 애니메이션
                if (pushable.TryGetComponent(out MonsterAnimator monsterAnimator))
                {
                    monsterAnimator.OnPushed(direction);
                }

                DestroyMonster(pushable, pushablePos);
                return false;
            }

            // Block은 벽으로 밀 수 없음 -> 움찔거리는 효과 적용
            if (pushable.CompareTag("Block") && pushable.TryGetComponent(out block))
            {
                block.OnBlocked(pushablePos);
            }

            return false;
        }

        // 밀릴 위치에 다른 밀 수 있는 오브젝트 있는지 체크
        GameObject anotherPushable = GridManager.Instance.GetPushableAt(pushTargetPos);
        if (anotherPushable != null)
        {
            // Block이 연속으로 있을 때 -> 움찔거리는 효과 적용
            if (pushable.CompareTag("Block") && pushable.TryGetComponent(out block))
            {
                block.OnBlocked(pushablePos);
            }
            return false; // Block이 연속으로 있으면 밀 수 없음
        }

        // 밀기 처리
        if (pushable.CompareTag("Block") && pushable.TryGetComponent(out block))
        {
            EffectManager.Instance.PlayEffectAtGrid(EffectType.Move, pushablePos);
            block.OnSlid(pushablePos, pushTargetPos);
            GridManager.Instance.MoveObject(pushable, pushablePos, pushTargetPos, updateTransform: false);
        }
        else
        {
            if (pushable.TryGetComponent(out MonsterAnimator monsterAnimator))
            {
                monsterAnimator.OnPushed(direction);
            }

            EffectManager.Instance.PlayEffectAtGrid(EffectType.Move, pushablePos);
            GridManager.Instance.MoveObject(pushable, pushablePos, pushTargetPos);
        }
        return true;
    }

    private void DestroyMonster(GameObject monster, Vector2Int monsterPos)
    {
        // 파괴 이펙트 재생
        EffectManager.Instance.PlayEffectAtGrid(EffectType.MonsterDestroy, monsterPos);

        GridManager.Instance.UnregisterObject(monster);
        Destroy(monster);
    }

    private void CollectKey(GameObject key)
    {
        GameManager.Instance.SetKey(true);
        EffectManager.Instance.PlayEffectAtObject(EffectType.KeyCollect, key);
        GridManager.Instance.UnregisterObject(key);
        Destroy(key);
    }

    private void UnlockBox(GameObject lockBox)
    {
        GridManager.Instance.UnregisterObject(lockBox);
        Destroy(lockBox);

        GameManager.Instance.SetKey(false);
    }
}
