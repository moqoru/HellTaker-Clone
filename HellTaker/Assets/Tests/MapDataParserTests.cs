using NUnit.Framework;
using UnityEngine;

public class MapDataParserTests
{
    // 테스트용 정상 CSV 헬퍼
    // 첫 줄: 메타데이터, 둘째 줄~: 3x3 타일
    private const string ValidCsv =
        "23,0,-0.5,PandeMonica\n" +
        "#,#,#\n" +
        "#,P,G\n" +
        "#,#,#";

    // === 정상 케이스 ===

    [Test]
    public void Parse_ValidCsv_ReturnsCorrectMetadata()
    {
        var result = MapDataParser.Parse(ValidCsv);

        Assert.AreEqual(23, result.MoveCount);
        Assert.AreEqual(new Vector2(0f, -0.5f), result.BasePosition);
        Assert.AreEqual("PandeMonica", result.GoalCharacter);
        Assert.IsFalse(result.IsGoalCharacterDefaulted);
    }

    [Test]
    public void Parse_ValidCsv_ReturnsCorrectTileData()
    {
        var result = MapDataParser.Parse(ValidCsv);

        Assert.AreEqual(3, result.Width);
        Assert.AreEqual(3, result.Height);

        // 중간 줄: #,P,G
        Assert.AreEqual("#", result.TileData[1, 0]);
        Assert.AreEqual("P", result.TileData[1, 1]);
        Assert.AreEqual("G", result.TileData[1, 2]);
    }

    [Test]
    public void Parse_GoalCharacterMissing_AppliesDefault()
    {
        // 골 캐릭터 컬럼 자체 누락
        string csv =
            "23,0,0\n" +
            "P,G";

        var result = MapDataParser.Parse(csv);

        Assert.AreEqual("PandeMonica", result.GoalCharacter);
        Assert.IsTrue(result.IsGoalCharacterDefaulted);
    }

    [Test]
    public void Parse_GoalCharacterEmpty_AppliesDefault()
    {
        // 골 캐릭터 컬럼이 빈 문자열
        string csv =
            "23,0,0,\n" +
            "P,G";

        var result = MapDataParser.Parse(csv);

        Assert.AreEqual("PandeMonica", result.GoalCharacter);
        Assert.IsTrue(result.IsGoalCharacterDefaulted);
    }

    [Test]
    public void Parse_GoalCharacterWithWhitespace_TrimsCorrectly()
    {
        string csv =
            "23,0,0,  Modeus  \n" +
            "P,G";

        var result = MapDataParser.Parse(csv);

        Assert.AreEqual("Modeus", result.GoalCharacter);
        Assert.IsFalse(result.IsGoalCharacterDefaulted);
    }

    // === 부정 케이스 (예외) ===

    [Test]
    public void Parse_EmptyString_ThrowsException()
    {
        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse(""));
    }

    [Test]
    public void Parse_WhitespaceOnly_ThrowsException()
    {
        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse("   \n  \n  "));
    }

    [Test]
    public void Parse_MetadataOnly_NoTileData_ThrowsException()
    {
        // 메타데이터는 있지만 타일 데이터가 없음
        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse("23,0,0,PandeMonica"));
    }

    [Test]
    public void Parse_MetadataInsufficientFields_ThrowsException()
    {
        // 메타데이터 필드 부족 (이동 횟수만 있음)
        string csv =
            "23\n" +
            "P,G";

        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse(csv));
    }

    [Test]
    public void Parse_InvalidMoveCount_ThrowsException()
    {
        string csv =
            "abc,0,0,PandeMonica\n" +
            "P,G";

        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse(csv));
    }

    [Test]
    public void Parse_InvalidOffsetX_ThrowsException()
    {
        string csv =
            "23,xyz,0,PandeMonica\n" +
            "P,G";

        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse(csv));
    }

    [Test]
    public void Parse_InvalidOffsetY_ThrowsException()
    {
        string csv =
            "23,0,xyz,PandeMonica\n" +
            "P,G";

        Assert.Throws<MapDataParseException>(() => MapDataParser.Parse(csv));
    }

    // === 엣지 케이스 ===

    [Test]
    public void Parse_BlankLinesInMiddle_IgnoresThem()
    {
        // 빈 줄이 중간에 끼어있어도 정상 파싱
        string csv =
            "23,0,0,PandeMonica\n" +
            "\n" +
            "#,#,#\n" +
            "   \n" +
            "P,.,G";

        var result = MapDataParser.Parse(csv);

        Assert.AreEqual(2, result.Height);
        Assert.AreEqual(3, result.Width);
        Assert.AreEqual("P", result.TileData[1, 0]);
    }

    [Test]
    public void Parse_NegativeOffset_ParsesCorrectly()
    {
        // 음수 오프셋 (실제 Stage1.csv가 -0.5 사용 중)
        string csv =
            "23,-1.5,-0.5,PandeMonica\n" +
            "P,G";

        var result = MapDataParser.Parse(csv);

        Assert.AreEqual(new Vector2(-1.5f, -0.5f), result.BasePosition);
    }

    [Test]
    public void Parse_CellsTrimmed_ReturnsTrimmedTileData()
    {
        // 셀에 공백 섞여있어도 trim됨
        string csv =
            "23,0,0,PandeMonica\n" +
            " # , P , G ";

        var result = MapDataParser.Parse(csv);

        Assert.AreEqual("#", result.TileData[0, 0]);
        Assert.AreEqual("P", result.TileData[0, 1]);
        Assert.AreEqual("G", result.TileData[0, 2]);
    }
}