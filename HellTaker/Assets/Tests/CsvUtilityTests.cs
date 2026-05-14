using NUnit.Framework;

public class CsvUtilityTests
{
    // === ParseLine ===

    [Test]
    public void ParseLine_SimpleLine_SplitsByComma()
    {
        string[] result = CsvUtility.ParseLine("가,나,다");

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual("가", result[0]);
        Assert.AreEqual("나", result[1]);
        Assert.AreEqual("다", result[2]);
    }

    [Test]
    public void ParseLine_CommaInsideQuotes_KeepsAsOneCell()
    {
        // 따옴표 안의 콤마는 셀 구분자가 아님
        string[] result = CsvUtility.ParseLine("\"안녕, 모코루\",응");

        Assert.AreEqual(2, result.Length);
        Assert.AreEqual("\"안녕, 모코루\"", result[0]);
        Assert.AreEqual("응", result[1]);
    }

    [Test]
    public void ParseLine_EmptyCells_PreservesEmpty()
    {
        // 끝쪽 빈 필드 보존 (대사 CSV 끝 ",," 패턴)
        string[] result = CsvUtility.ParseLine("안,,녕,");

        Assert.AreEqual(4, result.Length);
        Assert.AreEqual("안", result[0]);
        Assert.AreEqual("", result[1]);
        Assert.AreEqual("녕", result[2]);
        Assert.AreEqual("", result[3]);
    }

    // === CleanString ===

    [Test]
    public void CleanString_TripleQuotes_StripsCorrectly()
    {
        // 3중 따옴표는 엑셀이 큰따옴표가 포함된 셀을 저장할 때 만드는 패턴
        Assert.AreEqual("대사", CsvUtility.CleanString("\"\"\"대사\"\"\""));
    }


    [Test]
    public void CleanString_BackslashN_ConvertsToNewline()
    {
        // CSV 셀의 \n 문자열은 실제 줄바꿈으로
        Assert.AreEqual("줄1\n줄2", CsvUtility.CleanString("줄1\\n줄2"));
    }

    [Test]
    public void CleanString_EmptyInput_ReturnsEmpty()
    {
        Assert.AreEqual("", CsvUtility.CleanString(""));
        Assert.AreEqual("", CsvUtility.CleanString(null));
    }
}