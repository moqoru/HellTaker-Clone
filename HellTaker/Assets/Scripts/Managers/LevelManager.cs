using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [Header("�� ���� ����")]
    [Tooltip("�� ���� ���")]
    public string MapFileName = "Stages/Stage1";

    [Header("Ÿ�� ������")]
    public GameObject PlayerPrefab;
    public GameObject GoalPrefab;
    public GameObject WallPrefab;
    public GameObject BlockPrefab;
    public GameObject MonsterPrefab;

    [Header("�� ����")]
    public Vector2 basePosition = Vector2.zero;
    public float tileSize = 1f;

    private Transform goalParent;
    private Transform wallParent;
    private Transform blockParent;
    private Transform monsterParent;

    private string[,] mapData;
    private int mapWidth;
    private int mapHeight;

    // Ÿ�� Ÿ��
    public const char TILE_EMPTY = '.';
    public const char TILE_WALL = '#';
    public const char TILE_BLOCK = 'B';
    public const char TILE_PLAYER = 'P';
    public const char TILE_GOAL = 'G';
    public const char TILE_MONSTER = 'M';
    public const char TILE_THORN = 'T';
    public const char TILE_HIDDEN_THORN = 'H';
    public const char TILE_KEY = 'K';
    public const char TILE_LOCKBOX = 'L';

    void Start()
    {
        goalParent = new GameObject("Goals").transform;
        wallParent = new GameObject("Walls").transform;
        blockParent = new GameObject("Blocks").transform;
        monsterParent = new GameObject("Monsters").transform;

        LoadMapFromCSV();
        GenerateMap();
    }

    /** CSV ������ �а� 2D ���� �迭�� ��ȯ */
    void LoadMapFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(MapFileName);
        if (csvFile == null)
        {
            Debug.LogError($"�� ������ ã�� �� �����ϴ�: {MapFileName}");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        List<string> validLines = new List<string>();

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                validLines.Add(line.Trim());
            }
        }

        mapHeight = validLines.Count;
        mapWidth = validLines[0].Count(c => c == ',') + 1;

        mapData = new string[mapHeight, mapWidth];

        for (int y = 0; y < mapHeight; y++)
        {
            string[] cells = validLines[y].Split(',');
            for (int x = 0; x < cells.Length && x < mapWidth; x++)
            {
                mapData[y, x] = cells[x].Trim();
            }
        }

        Debug.Log($"�� �ε� �Ϸ�, ũ��: {mapWidth}x{mapHeight}");

    }

    /** �ε�� �� �����͸� ������� ������Ʈ ���� */
    void GenerateMap()
    {
        if(mapData == null)
        {
            Debug.LogError("�� �����Ͱ� �����ϴ�.");
            return;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // ������ �߽� ������ �������� ��ġ ����
                Vector3 spawnPosition = new Vector3(
                    basePosition.x + (x - mapWidth / 2) * tileSize,
                    basePosition.y - (y - mapHeight / 2) * tileSize,
                    0
                );

                string tileData = mapData[y, x];
                if (tileData.Contains(TILE_PLAYER))
                    Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity, null);
                if (tileData.Contains(TILE_GOAL))
                    Instantiate(GoalPrefab, spawnPosition, Quaternion.identity, goalParent);
                if (tileData.Contains(TILE_WALL))
                    Instantiate(WallPrefab, spawnPosition, Quaternion.identity, wallParent);
                if (tileData.Contains(TILE_BLOCK))
                    Instantiate(BlockPrefab, spawnPosition, Quaternion.identity, blockParent);
                if (tileData.Contains(TILE_MONSTER))
                    Instantiate(MonsterPrefab, spawnPosition, Quaternion.identity, monsterParent);
            }
        }

        Debug.Log("�� ���� �Ϸ�");
    }
}
