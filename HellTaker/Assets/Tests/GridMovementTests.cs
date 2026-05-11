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

    // RegisterObject / IsPositionBlocked Å×½ºÆ®
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
}