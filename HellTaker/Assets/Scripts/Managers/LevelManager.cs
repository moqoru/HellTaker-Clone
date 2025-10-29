using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [Header("맵 파일 설정")]
    [Tooltip("맵 파일 경로")]
    public string MapFileName = "Stages/Stage1";

    [Header("타일 프리팹")]
    public GameObject PlayerPrefab;
    public GameObject GoalPrefab;
    public GameObject WallPrefab;
    public GameObject BlockPrefab;
    public GameObject MonsterPrefab;

    [Header("맵 설정")]
    public Vector2 basePosition = Vector2.zero;
    public float tileSize = 1f;

    private Transform goalParent;
    private Transform wallParent;
    private Transform blockParent;
    private Transform monsterParent;

    private string[,] mapData;
    private int mapWidth;
    private int mapHeight;

    // 타일 타입
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

    /** CSV 파일을 읽고 2D 문자 배열로 변환 */
    void LoadMapFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(MapFileName);
        if (csvFile == null)
        {
            Debug.LogError($"맵 파일을 찾을 수 없습니다: {MapFileName}");
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

        Debug.Log($"맵 로드 완료, 크기: {mapWidth}x{mapHeight}");

    }

    /** 로드된 맵 데이터를 기반으로 오브젝트 생성 */
    void GenerateMap()
    {
        if(mapData == null)
        {
            Debug.LogError("맵 데이터가 없습니다.");
            return;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // 파일의 중심 지점을 기준으로 위치 지정
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

        Debug.Log("맵 생성 완료");
    }
}
