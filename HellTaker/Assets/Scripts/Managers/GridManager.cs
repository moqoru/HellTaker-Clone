using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    // 위치별 오브젝트 딕셔너리 (key : 그리드 좌표, value : 해당 위치의 오브젝트 리스트)
    private Dictionary<Vector2Int, List<GameObject>> grid = new Dictionary<Vector2Int, List<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // 싱글톤 중복 방지
        else if (Instance != this) 
        {
            Destroy(gameObject);
        }
    }

    /** 오브젝트를 그리드에 등록 */
    public void RegisterObject(GameObject obj)
    {
        Vector2Int gridPos = WorldToGrid(obj.transform.position);

        if (!grid.ContainsKey(gridPos))
        {
            grid[gridPos] = new List<GameObject>();
        }

        if (!grid[gridPos].Contains(obj))
        {
            grid[gridPos].Add(obj);
        }
    }

    /** 오브젝트를 그리드에서 제거 */
    public void UnregisterObject(GameObject obj)
    {
        Vector2Int gridPos = WorldToGrid(obj.transform.position);

        if (grid.ContainsKey(gridPos))
        {
            grid[gridPos].Remove(obj);

            if (grid[gridPos].Count == 0)
            {
                grid.Remove(gridPos);
            }
        }
    }

    /// <summary>
    /// 오브젝트를 한 위치에서 다른 위치로 이동
    /// <para> !!!주의!!! 이동 전에 IsPositionBlocked를 먼저 수행해야 합니다. </para>
    /// </summary>
    public void MoveObject(GameObject obj, Vector2Int fromPos, Vector2Int toPos)
    {
        // 기존 위치에서 제거
        if (grid.ContainsKey(fromPos))
        {
            grid[fromPos].Remove(obj);
            if (grid[fromPos].Count == 0)
            {
                grid.Remove(fromPos);
            }
        }

        // 이동할 위치에 추가
        if (!grid.ContainsKey(toPos))
        {
            grid[toPos] = new List<GameObject>();
        }

        if (!grid[toPos].Contains(obj))
        {
            grid[toPos].Add(obj);
        }

        obj.transform.position = GridToWorld(toPos);
    }

    /** 특정 위치가 막혀있는지 체크*/
    public bool IsPositionBlocked(Vector2Int pos)
    {
        if (!grid.ContainsKey(pos))
        {
            return false;
        }

        foreach (GameObject obj in grid[pos])
        {
            if (obj == null) continue;

            if (obj.CompareTag("Wall")
                || obj.CompareTag("Goal")
                || obj.CompareTag("LockBox"))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPositionPunished(Vector2Int pos)
    {
        if (!grid.ContainsKey(pos))
        {
            return false;
        }

        foreach (GameObject obj in grid[pos])
        {
            if (obj == null) continue;

            if (obj.CompareTag("ThornNormal")
                || obj.CompareTag("ThornHidden"))
            {
                return true;
            }
        }

        return false;

    }

    /** 특정 위치의 모든 오브젝트 확인 */
    public List<GameObject> GetObjectsAt(Vector2Int pos)
    {
        if (!grid.ContainsKey(pos))
        {
            return new List<GameObject>();
        }

        List<GameObject> validObjects = new List<GameObject>();
        foreach (GameObject obj in grid[pos])
        {
            if (obj != null)
            {
                validObjects.Add(obj);
            }
        }

        return validObjects;
    }

    /** 특정 위치의 특정 태그 오브젝트 확인 */
    public GameObject GetObjectWithTagAt(Vector2Int pos, string tag)
    {
        List<GameObject> objects = GetObjectsAt(pos);
        foreach (GameObject obj in objects)
        {
            if (obj.CompareTag(tag))
            {
                return obj;
            }
        }

        return null;
    }

    /** 특정 위치에 밀 수 있는 오브젝트 있는지 확인 */
    public GameObject GetPushableAt(Vector2Int pos)
    {
        List<GameObject> objects = GetObjectsAt(pos);
        foreach (GameObject obj in objects)
        {
            if (obj.CompareTag("Block")
                || obj.CompareTag("Monster"))
            {
                return obj;
            }
        }

        return null;
    }

    /** 월드 좌표를 그리드 좌표로 변환 (=> 정수) */
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector2 basePos = LevelManager.Instance.GetBasePosition();

        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x - basePos.x),
            Mathf.RoundToInt(worldPos.y - basePos.y)
        );
    }

    /** 그리드 좌표를 월드 좌표로 변환 (=> 실수) */
    public Vector3 GridToWorld(Vector2 gridPos)
    {
        Vector2 basePos = LevelManager.Instance.GetBasePosition();

        return new Vector3(
            gridPos.x + basePos.x,
            gridPos.y + basePos.y,
            0
        );
    }

    /** 그리드 전체 초기화 (레벨 전환 시) */
    public void ClearGrid()
    {
        grid.Clear();
    }

    /** (디버깅) 현재 그리드 상태 출력 */
    public void DebugPrintGrid()
    {
        Debug.Log($"=== 만들어진 그리드 칸 수: {grid.Count} ===");
        foreach (var gridKVPair in grid)
        {
            string objNames = "";
            foreach (GameObject obj in gridKVPair.Value)
            {
                if (obj != null)
                {
                    objNames += obj.name + ", ";
                }
            }
            Debug.Log($"{gridKVPair.Key}의 값: {objNames}");
        }
    }
}
