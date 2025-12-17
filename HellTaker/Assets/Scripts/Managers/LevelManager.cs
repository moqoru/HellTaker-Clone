using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

using static DestroyHelper;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("타일 프리팹")]
    public GameObject PlayerPrefab;
    public GameObject GoalPrefab;
    public GameObject WallPrefab;
    public GameObject BlockPrefab;
    public GameObject MonsterPrefab;
    public GameObject ThornNormalPrefab;
    public GameObject ThornTogglePrefab;
    public GameObject KeyPrefab;
    public GameObject LockBoxPrefab;

    [Header("배경 이미지")]
    public SpriteRenderer backGroundImage;

    [Header("맵 설정")]
    public Vector2 basePosition = Vector2.zero;
    public float tileSize = 1f;

    private Transform goalParent;
    private Transform wallParent;
    private Transform blockParent;
    private Transform monsterParent;
    private Transform thornNormalParent;
    private Transform thornToggleParent;

    private string MapFileName;
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
    public const char TILE_THORN_NORMAL = 'T';
    public const char TILE_THORN_UP = 'U';
    public const char TILE_THORN_DOWN = 'D';
    public const char TILE_KEY = 'K';
    public const char TILE_LOCKBOX = 'L';

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

    private void Start()
    {
        MapFileName = $"Stages/Stage{GameManager.Instance.currentStage}";
        backGroundImage.sprite = Resources.Load<Sprite>($"BackGround/ChapterBG_00{GameManager.Instance.currentStage}");
        // TODO: Epilogue는 번호 다르게 설정

        goalParent = new GameObject("Goals").transform;
        wallParent = new GameObject("Walls").transform;
        blockParent = new GameObject("Blocks").transform;
        monsterParent = new GameObject("Monsters").transform;
        thornNormalParent = new GameObject("ThornsNormal").transform;
        thornToggleParent = new GameObject("ThornsToggle").transform;

        LoadMapFromCSV();
        GenerateMap();
    }

    /** CSV 파일을 읽고 2D 문자 배열로 변환 */
    private void LoadMapFromCSV()
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

        mapHeight = validLines.Count - 1;
        mapWidth = validLines[1].Count(c => c == ',') + 1;

        string[] mapInfo = validLines[0].Split(',');

        if (int.TryParse(mapInfo[0].Trim(), out int moveCount))
        {
            GameManager.Instance.maxMoveCount = moveCount;
        }
        else
        {
            Debug.LogError($"이동 횟수를 불러오지 못했습니다: {mapInfo[0]}");
        }

        if (float.TryParse(mapInfo[1].Trim(), out float offsetX)
            && float.TryParse(mapInfo[2].Trim(), out float offsetY))
        {
            basePosition.x = offsetX;
            basePosition.y = offsetY;
        }
        else
        {
            Debug.LogError($"오프셋 좌표를 불러오지 못했습니다: x => {mapInfo[1]} y => {mapInfo[2]}");
        }

        mapData = new string[mapHeight, mapWidth];

        for (int y = 0; y < mapHeight; y++)
        {
            string[] cells = validLines[y + 1].Split(',');
            for (int x = 0; x < cells.Length && x < mapWidth; x++)
            {
                mapData[y, x] = cells[x].Trim();
            }
        }

    }

    /** 로드된 맵 데이터를 기반으로 오브젝트 생성 */
    private void GenerateMap()
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
                GameObject spawnedObject;

                if (tileData.Contains(TILE_PLAYER))
                {
                    spawnedObject = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity, null);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_KEY))
                {
                    spawnedObject = Instantiate(KeyPrefab, spawnPosition, Quaternion.identity, null);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_LOCKBOX))
                {
                    spawnedObject = Instantiate(LockBoxPrefab, spawnPosition, Quaternion.identity, null);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_THORN_NORMAL))
                {
                    spawnedObject = Instantiate(ThornNormalPrefab, spawnPosition, Quaternion.identity, thornNormalParent);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_THORN_UP))
                {
                    spawnedObject = Instantiate(ThornTogglePrefab, spawnPosition, Quaternion.identity, thornToggleParent);
                    spawnedObject.tag = "ThornUp";
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_THORN_DOWN))
                {
                    spawnedObject = Instantiate(ThornTogglePrefab, spawnPosition, Quaternion.identity, thornToggleParent);
                    spawnedObject.tag = "ThornDown";
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_GOAL))
                {
                    spawnedObject = Instantiate(GoalPrefab, spawnPosition, Quaternion.identity, goalParent);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_WALL))
                {
                    spawnedObject = Instantiate(WallPrefab, spawnPosition, Quaternion.identity, wallParent);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_BLOCK))
                {
                    spawnedObject = Instantiate(BlockPrefab, spawnPosition, Quaternion.identity, blockParent);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
                if (tileData.Contains(TILE_MONSTER))
                {
                    spawnedObject = Instantiate(MonsterPrefab, spawnPosition, Quaternion.identity, monsterParent);
                    GridManager.Instance.RegisterObject(spawnedObject);
                }
            }
        }

    }

    /** 현재 맵의 모든 오브젝트 제거 */
    public void ClearCurrentMap()
    {
        // 모든 부모 오브젝트의 자식 삭제
        DestroyAllChildren(goalParent,
            wallParent,
            blockParent,
            monsterParent,
            thornNormalParent,
            thornToggleParent);

        // 단일 오브젝트들도 함께 삭제
        DestroyAllWithTagImmediate("Player", "Key", "LockBox");

        // Grid 초기화
        GridManager.Instance.ClearGrid();
    }

    /** 스테이지 리로드 */
    public void ReloadStage()
    {
        ClearCurrentMap();
        
        // TODO: 턴 정보, 오프셋 정보도 CSV에 담아 설정하기
        LoadMapFromCSV();
        GenerateMap();
    }

    /** 다음 스테이지 로드 */
    public void LoadNextStage(int stageNum)
    {
        ClearCurrentMap();

        // 맵, 배경 이미지 파일명 재설정
        MapFileName = $"Stages/Stage{stageNum}";
        backGroundImage.sprite = Resources.Load<Sprite>($"BackGround/ChapterBG_00{GameManager.Instance.currentStage}");
        // TODO: Epilogue는 번호 다르게 설정

        // TODO: 오프셋 정보도 CSV에 담아 설정하기
        LoadMapFromCSV();
        GenerateMap();
    }

    /** basePosition 전달 */
    public Vector2 GetBasePosition()
    {
        return basePosition;
    }

    /** (디버깅) 특정 좌표의 타일 문자 확인 */
    public string GetTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
        {
            return ".";
        }
        return mapData[y, x];
    }

    /** (디버깅) 맵 크기 확인 */
    public Vector2Int GetMapSize()
    {
        return new Vector2Int(mapWidth, mapHeight);
    }
}
