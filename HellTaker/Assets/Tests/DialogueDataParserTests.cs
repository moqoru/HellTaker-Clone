using NUnit.Framework;

public class DialogueDataParserTests
{
    private const string Header = "DialogueID,Type,CharacterImage,CharacterName,Text,Choice1,Choice1Next,Choice2,Choice2Next,Choice3,Choice3Next";

    // === 정상 케이스 ===

    [Test]
    public void Parse_DialogueNode_AutoIncrementsNextID()
    {
        string csv = Header + "\n" +
            "5,Dialogue,Scene/Pand,판데모니카,\"대사 텍스트\",,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual(1, result.Nodes.Count);
        Assert.IsTrue(result.Nodes.ContainsKey(5));
        Assert.AreEqual(DialogueType.Dialogue, result.Nodes[5].type);
        Assert.AreEqual("판데모니카", result.Nodes[5].characterName);
        Assert.AreEqual(6, result.Nodes[5].nextDialogueID); // id + 1 자동
    }

    [Test]
    public void Parse_ChoiceNode_ParsesThreeChoicesAndCorrectIndex()
    {
        // 두 번째 선택지가 정답 (nextID=2, 임계값 100 미만)
        string csv = Header + "\n" +
            "1,Choice,Scene/Pand,판데모니카,\"모코루?\",\"오답1\",101,\"정답\",2,\"오답2\",102";

        var result = DialogueDataParser.Parse(csv);
        var node = result.Nodes[1];

        Assert.AreEqual(DialogueType.Choice, node.type);
        Assert.AreEqual("오답1", node.choiceTexts[0]);
        Assert.AreEqual(101, node.choiceNextIDs[0]);
        Assert.AreEqual("정답", node.choiceTexts[1]);
        Assert.AreEqual(2, node.choiceNextIDs[1]);
        Assert.AreEqual(1, node.correctChoiceIndex); // 두 번째가 정답
    }

    [Test]
    public void Parse_CutSceneNode_AutoIncrementsNextID()
    {
        string csv = Header + "\n" +
            "301,CutScene,Scene/Cut,,\"오프닝 텍스트\",,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual(DialogueType.CutScene, result.Nodes[301].type);
        Assert.AreEqual(302, result.Nodes[301].nextDialogueID);
    }

    [Test]
    public void Parse_NumberChoiceNode_ParsesMinMaxAndNextIDs()
    {
        string csv = Header + "\n" +
            "205,NumberChoice,Scene/Pand,판데모니카,\"몇 점?\",1,206,10,207,,";

        var result = DialogueDataParser.Parse(csv);
        var node = result.Nodes[205];

        Assert.AreEqual(DialogueType.NumberChoice, node.type);
        Assert.AreEqual(1, node.minValue);
        Assert.AreEqual(10, node.maxValue);
        Assert.AreEqual(206, node.choiceNextIDs[0]);
        Assert.AreEqual(207, node.choiceNextIDs[1]);
    }

    // === 부정 케이스 (줄 단위 무시 + Warnings 누적) ===

    [Test]
    public void Parse_InvalidID_SkipsLineAndWarns()
    {
        string csv = Header + "\n" +
            "가나다,Dialogue,,,텍스트,,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual(0, result.Nodes.Count);
        Assert.AreEqual(1, result.Warnings.Count);
        StringAssert.Contains("ID 파싱 실패", result.Warnings[0]);
    }

    [Test]
    public void Parse_InvalidType_SkipsLineAndWarns()
    {
        string csv = Header + "\n" +
            "1,NotAType,,,텍스트,,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual(0, result.Nodes.Count);
        Assert.AreEqual(1, result.Warnings.Count);
        StringAssert.Contains("타입 파싱 실패", result.Warnings[0]);
    }

    [Test]
    public void Parse_InsufficientFields_SkipsLineAndWarns()
    {
        // 필드 5개 미만
        string csv = Header + "\n" +
            "1,Dialogue,Scene/Pand";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual(0, result.Nodes.Count);
        Assert.AreEqual(1, result.Warnings.Count);
        StringAssert.Contains("필드 수 부족", result.Warnings[0]);
    }

    [Test]
    public void Parse_MixedValidAndInvalid_LoadsValidOnly()
    {
        // 유효한 줄과 깨진 줄이 섞여있을 때 유효한 것만 로드
        string csv = Header + "\n" +
            "1,Dialogue,,,첫째,,,,,,\n" +
            "가나다,Dialogue,,,깨진ID,,,,,,\n" +
            "3,Dialogue,,,셋째,,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual(2, result.Nodes.Count);
        Assert.IsTrue(result.Nodes.ContainsKey(1));
        Assert.IsTrue(result.Nodes.ContainsKey(3));
        Assert.AreEqual(1, result.Warnings.Count);
    }

    // === 부정 케이스 (전체 예외) ===

    [Test]
    public void Parse_EmptyString_ThrowsException()
    {
        Assert.Throws<DialogueDataParseException>(() => DialogueDataParser.Parse(""));
    }

    [Test]
    public void Parse_WhitespaceOnly_ThrowsException()
    {
        Assert.Throws<DialogueDataParseException>(() => DialogueDataParser.Parse("   \n  "));
    }

    [Test]
    public void Parse_HeaderOnly_ReturnsEmptyNodes()
    {
        // 헤더만 있고 데이터 없음 → 예외 아니라 빈 Dictionary 반환
        var result = DialogueDataParser.Parse(Header);

        Assert.AreEqual(0, result.Nodes.Count);
        Assert.AreEqual(0, result.Warnings.Count);
    }

    // === 엣지 케이스 ===

    [Test]
    public void Parse_TextWithCommaInsideQuotes_PreservesText()
    {
        // 따옴표 안 콤마는 텍스트로 처리
        string csv = Header + "\n" +
            "1,Dialogue,,,\"안녕, 모코루\",,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual("안녕, 모코루", result.Nodes[1].text);
    }

    [Test]
    public void Parse_TextWithEscapedNewline_ConvertsToActualNewline()
    {
        // \n 이스케이프 → 실제 줄바꿈
        string csv = Header + "\n" +
            "1,Dialogue,,,\"줄1\\n줄2\",,,,,,";

        var result = DialogueDataParser.Parse(csv);

        Assert.AreEqual("줄1\n줄2", result.Nodes[1].text);
    }
}