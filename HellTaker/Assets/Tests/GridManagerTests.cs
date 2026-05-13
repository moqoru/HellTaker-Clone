using NUnit.Framework;
using UnityEngine;

public class GridManagerTests
{
    private GridManager gridManager;
    private Vector2 testBasePos = Vector2.zero;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        gridManager = go.AddComponent<GridManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gridManager.gameObject);
    }

    // RegisterObject / IsPositionBlocked 테스트
    [Test]
    public void IsPositionBlocked_WallTag_ReturnsTrue()
    {
        var wall = new GameObject();
        wall.tag = "Wall";
        wall.transform.position = Vector3.zero;

        gridManager.RegisterObject(wall, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(Vector3.zero, testBasePos);
        Assert.IsTrue(gridManager.IsPositionBlocked(gridPos));

        Object.DestroyImmediate(wall);
    }

    [Test]
    public void IsPositionBlocked_EmptyPos_ReturnsFalse()
    {
        Vector2Int gridPos = new Vector2Int(99, 99);
        Assert.IsFalse(gridManager.IsPositionBlocked(gridPos));
    }

    [Test]
    public void IsPositionPunished_ThornNormalTag_ReturnsTrue()
    {
        var thorn = new GameObject();
        thorn.tag = "ThornNormal";
        thorn.transform.position = new Vector3(1, 0, 0);

        gridManager.RegisterObject(thorn, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(new Vector3(1, 0, 0), testBasePos);
        Assert.IsTrue(gridManager.IsPositionPunished(gridPos));

        Object.DestroyImmediate(thorn);
    }

    [Test]
    public void MoveObject_UpdatesDictionary()
    {
        var block = new GameObject();
        block.tag = "Block";
        block.transform.position = Vector3.zero;

        gridManager.RegisterObject(block, testBasePos);

        Vector2Int fromPos = gridManager.WorldToGrid(Vector3.zero, testBasePos);
        Vector2Int toPos = new Vector2Int(1, 0);

        gridManager.MoveObject(block, fromPos, toPos, updateTransform: false);

        Assert.IsFalse(gridManager.IsPositionBlocked(fromPos));
        Assert.IsNotNull(gridManager.GetPushableAt(toPos));

        Object.DestroyImmediate(block);
    }

    [Test]
    public void WorldToGrid_ZeroBase_ReturnsCorrectGrid()
    {
        Vector3 worldPos = new Vector3(3, -2, 0);
        Vector2Int result = gridManager.WorldToGrid(worldPos, testBasePos);

        Assert.AreEqual(new Vector2Int(3, -2), result);
    }

    [Test]
    public void GridToWorld_ZeroBase_ReturnsCorrectWorld()
    {
        Vector2 gridPos = new Vector2(2, 4);
        Vector3 result = gridManager.GridToWorld(gridPos, testBasePos);

        Assert.AreEqual(new Vector3(2, 4, 0), result);
    }

    // ClearGrid 초기화 검증 — 다른 테스트의 격리 보장 차원에서 가장 먼저 검증
    [Test]
    public void ClearGrid_RemovesAllObjects()
    {
        var wall = new GameObject { tag = "Wall" };
        wall.transform.position = Vector3.zero;
        gridManager.RegisterObject(wall, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(Vector3.zero, testBasePos);
        Assert.IsTrue(gridManager.IsPositionBlocked(gridPos), "사전 조건: 등록 직후엔 막혀있어야 함");

        gridManager.ClearGrid();

        Assert.IsFalse(gridManager.IsPositionBlocked(gridPos), "ClearGrid 이후엔 빈 칸이어야 함");

        Object.DestroyImmediate(wall);
    }

    // IsPositionBlocked - Goal 태그 검증
    [Test]
    public void IsPositionBlocked_GoalTag_ReturnsTrue()
    {
        var goal = new GameObject { tag = "Goal" };
        goal.transform.position = new Vector3(2, 3, 0);
        gridManager.RegisterObject(goal, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(new Vector3(2, 3, 0), testBasePos);
        Assert.IsTrue(gridManager.IsPositionBlocked(gridPos));

        Object.DestroyImmediate(goal);
    }

    // IsPositionPunished - ThornUp 태그 검증 (ThornNormal은 기존 테스트가 커버)
    [Test]
    public void IsPositionPunished_ThornUpTag_ReturnsTrue()
    {
        var thornUp = new GameObject { tag = "ThornUp" };
        thornUp.transform.position = new Vector3(1, 1, 0);
        gridManager.RegisterObject(thornUp, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(new Vector3(1, 1, 0), testBasePos);
        Assert.IsTrue(gridManager.IsPositionPunished(gridPos));

        Object.DestroyImmediate(thornUp);
    }

    // GetObjectWithTagAt - 특정 태그 찾기
    [Test]
    public void GetObjectWithTagAt_ReturnsCorrectObject()
    {
        var key = new GameObject("TestKey") { tag = "Key" };
        key.transform.position = new Vector3(4, 0, 0);
        gridManager.RegisterObject(key, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(new Vector3(4, 0, 0), testBasePos);

        GameObject found = gridManager.GetObjectWithTagAt(gridPos, "Key");
        Assert.IsNotNull(found, "Key 태그 오브젝트를 찾아야 함");
        Assert.AreEqual("TestKey", found.name);

        // 다른 태그로 찾으면 null
        GameObject notFound = gridManager.GetObjectWithTagAt(gridPos, "Wall");
        Assert.IsNull(notFound, "존재하지 않는 태그는 null 반환");

        Object.DestroyImmediate(key);
    }

    // GetPushableAt - Block/Monster/LockBox 셋 다 인식하는지
    // TestCase로 3개 태그 한 번에 검증
    [TestCase("Block")]
    [TestCase("Monster")]
    [TestCase("LockBox")]
    public void GetPushableAt_PushableTags_ReturnsObject(string pushableTag)
    {
        var obj = new GameObject($"Test_{pushableTag}") { tag = pushableTag };
        obj.transform.position = new Vector3(5, 5, 0);
        gridManager.RegisterObject(obj, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(new Vector3(5, 5, 0), testBasePos);
        GameObject pushable = gridManager.GetPushableAt(gridPos);

        Assert.IsNotNull(pushable, $"{pushableTag} 태그는 푸시 가능해야 함");
        Assert.AreEqual(pushableTag, pushable.tag);

        Object.DestroyImmediate(obj);
    }

    // GetPushableAt - 푸시 불가능한 태그는 null 반환
    [Test]
    public void GetPushableAt_NonPushableTag_ReturnsNull()
    {
        var wall = new GameObject { tag = "Wall" };
        wall.transform.position = Vector3.zero;
        gridManager.RegisterObject(wall, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(Vector3.zero, testBasePos);
        Assert.IsNull(gridManager.GetPushableAt(gridPos));

        Object.DestroyImmediate(wall);
    }

    // UnregisterObject - 제거 후 빈칸 처리
    [Test]
    public void UnregisterObject_RemovesFromGrid()
    {
        var block = new GameObject { tag = "Block" };
        block.transform.position = new Vector3(3, 3, 0);
        gridManager.RegisterObject(block, testBasePos);

        Vector2Int gridPos = gridManager.WorldToGrid(new Vector3(3, 3, 0), testBasePos);
        Assert.IsNotNull(gridManager.GetPushableAt(gridPos), "사전 조건: 등록 직후엔 존재해야 함");

        gridManager.UnregisterObject(block, testBasePos);

        Assert.IsNull(gridManager.GetPushableAt(gridPos), "Unregister 이후엔 빈칸이어야 함");

        Object.DestroyImmediate(block);
    }
}