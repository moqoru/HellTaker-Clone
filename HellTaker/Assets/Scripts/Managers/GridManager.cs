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

    public void ToggleThorns()
    {
        foreach (var kvp in grid)
        {
            foreach (GameObject obj in kvp.Value)
            {
                if (obj == null) continue;

                ThornToggle thorn = obj.GetComponent<ThornToggle>();

                if (thorn != null)
                {
                    thorn.Toggle();
                }

            }
        }
    }

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
                || obj.CompareTag("ThornUp"))
            {
                return true;
            }
        }

        return false;

    }

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

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector2 basePos = LevelManager.Instance.GetBasePosition();

        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x - basePos.x),
            Mathf.RoundToInt(worldPos.y - basePos.y)
        );
    }

    public Vector3 GridToWorld(Vector2 gridPos)
    {
        Vector2 basePos = LevelManager.Instance.GetBasePosition();

        return new Vector3(
            gridPos.x + basePos.x,
            gridPos.y + basePos.y,
            0
        );
    }

    public void ClearGrid()
    {
        grid.Clear();
    }

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
