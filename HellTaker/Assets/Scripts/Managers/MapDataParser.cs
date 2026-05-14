using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 맵 CSV 텍스트를 파싱하여 ParsedMap 구조체로 반환하는 정적 클래스.
/// LevelManager의 LoadMapFromCSV에서 파싱 로직만 분리하여 단위 테스트가 가능하도록 함.
/// </summary>
public static class MapDataParser
{
    /// <summary>맵 CSV 텍스트 파싱 결과.</summary>
    public struct ParsedMap
    {
        public int MoveCount;
        public Vector2 BasePosition;
        public string GoalCharacter;
        public bool IsGoalCharacterDefaulted; // 메타데이터 누락으로 디폴트 적용된 경우 true
        public string[,] TileData;
        public int Width;
        public int Height;
    }

    private const string DEFAULT_GOAL_CHARACTER = "PandeMonica";

    /// <summary>
    /// 맵 CSV 텍스트를 파싱.
    /// 첫 줄: 메타데이터 (이동 횟수, offsetX, offsetY, [골 캐릭터])
    /// 둘째 줄 이후: 타일 데이터
    /// </summary>
    /// <exception cref="MapDataParseException">
    /// 치명적 파싱 실패 시 (빈 입력, 데이터 줄 없음, 메타데이터 필수 필드 파싱 실패).
    /// </exception>
    public static ParsedMap Parse(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            throw new MapDataParseException("CSV 텍스트가 비어있습니다.");
        }

        string[] lines = csvText.Split('\n');
        List<string> validLines = new List<string>();

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                validLines.Add(line.Trim());
            }
        }

        if (validLines.Count < 2)
        {
            throw new MapDataParseException(
                $"CSV에 데이터 줄이 없습니다. 유효 줄 수: {validLines.Count} (메타데이터 1줄 + 타일 데이터 최소 1줄 필요)");
        }

        // 메타데이터 파싱
        string[] mapInfo = validLines[0].Split(',');

        if (mapInfo.Length < 3)
        {
            throw new MapDataParseException(
                $"메타데이터가 부족합니다. 필요: 이동 횟수, offsetX, offsetY (최소 3개). 실제: {mapInfo.Length}개");
        }

        if (!int.TryParse(mapInfo[0].Trim(), out int moveCount))
        {
            throw new MapDataParseException(
                $"이동 횟수를 파싱할 수 없습니다: '{mapInfo[0]}'");
        }

        // 로케일 의존성 제거를 위해 InvariantCulture 명시
        if (!float.TryParse(mapInfo[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float offsetX)
            || !float.TryParse(mapInfo[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float offsetY))
        {
            throw new MapDataParseException(
                $"오프셋 좌표를 파싱할 수 없습니다: x => '{mapInfo[1]}', y => '{mapInfo[2]}'");
        }

        // 골 캐릭터는 누락되면 디폴트 + 플래그
        string goalCharacter;
        bool isGoalDefaulted;
        if (mapInfo.Length > 3 && !string.IsNullOrWhiteSpace(mapInfo[3]))
        {
            goalCharacter = mapInfo[3].Trim();
            isGoalDefaulted = false;
        }
        else
        {
            goalCharacter = DEFAULT_GOAL_CHARACTER;
            isGoalDefaulted = true;
        }

        // 타일 데이터 파싱
        int height = validLines.Count - 1;
        int width = validLines[1].Split(',').Length;

        string[,] tileData = new string[height, width];
        for (int y = 0; y < height; y++)
        {
            string[] cells = validLines[y + 1].Split(',');
            for (int x = 0; x < cells.Length && x < width; x++)
            {
                tileData[y, x] = cells[x].Trim();
            }
        }

        return new ParsedMap
        {
            MoveCount = moveCount,
            BasePosition = new Vector2(offsetX, offsetY),
            GoalCharacter = goalCharacter,
            IsGoalCharacterDefaulted = isGoalDefaulted,
            TileData = tileData,
            Width = width,
            Height = height
        };
    }
}

/// <summary>맵 CSV 파싱이 치명적으로 실패했을 때 발생하는 예외.</summary>
public class MapDataParseException : Exception
{
    public MapDataParseException(string message) : base(message) { }
}