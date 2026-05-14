using System;
using System.Collections.Generic;

/// <summary>
/// 대사 CSV 텍스트를 파싱하여 DialogueNode 딕셔너리로 반환.
/// 줄 단위 파싱 실패는 Warnings에 누적하고 건너뜀.
/// 전체 파싱 불가(빈 입력 등)는 예외.
/// </summary>
public static class DialogueDataParser
{
    /// <summary>대사 CSV 파싱 결과.</summary>
    public struct ParsedDialogue
    {
        public Dictionary<int, DialogueNode> Nodes;
        public List<string> Warnings; // 건너뛴 줄의 사유 누적
    }

    // ID 구분 임계값 (DialogueManager와 동일 — 정답 선택지 판정용)
    private const int DIALOGUE_THRESHOLD_ID = 100;

    // CSV 필드 최소 개수 (ID, Type, CharacterImage, CharacterName, Text)
    private const int MIN_FIELD_COUNT = 5;

    /// <summary>
    /// 대사 CSV 텍스트를 파싱.
    /// 첫 줄은 헤더로 간주하여 건너뜀.
    /// </summary>
    /// <exception cref="DialogueDataParseException">CSV가 비었거나 데이터 줄이 전혀 없을 때.</exception>
    public static ParsedDialogue Parse(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            throw new DialogueDataParseException("대사 CSV 텍스트가 비어있습니다.");
        }

        var nodes = new Dictionary<int, DialogueNode>();
        var warnings = new List<string>();

        string[] lines = csvText.Split('\n');

        // 0번째 줄은 헤더 → 건너뜀
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = CsvUtility.ParseLine(lines[i]);

            if (values.Length < MIN_FIELD_COUNT)
            {
                warnings.Add($"줄 {i}: 필드 수 부족 (필요: {MIN_FIELD_COUNT}, 실제: {values.Length})");
                continue;
            }

            if (!int.TryParse(values[0], out int id))
            {
                warnings.Add($"줄 {i}: ID 파싱 실패 ('{values[0]}')");
                continue;
            }

            if (!Enum.TryParse(values[1], out DialogueType type))
            {
                warnings.Add($"줄 {i}: 타입 파싱 실패 ('{values[1]}')");
                continue;
            }

            DialogueNode node = new DialogueNode
            {
                dialogueID = id,
                type = type,
                characterImagePath = CsvUtility.CleanString(values[2]),
                characterName = CsvUtility.CleanString(values[3]),
                text = CsvUtility.CleanString(values[4])
            };

            try
            {
                PopulateTypeSpecificFields(node, values);
            }
            catch (FormatException e)
            {
                warnings.Add($"줄 {i}: 타입별 필드 파싱 실패 ({e.Message})");
                continue;
            }

            nodes[node.dialogueID] = node;
        }

        return new ParsedDialogue
        {
            Nodes = nodes,
            Warnings = warnings
        };
    }

    /// <summary>
    /// 타입별 추가 필드 채우기.
    /// Choice: 선택지 3개 + correctChoiceIndex 판정
    /// NumberChoice: minValue, maxValue, choiceNextIDs[0..1]
    /// Dialogue/Advice/CutScene: nextDialogueID = id + 1
    /// </summary>
    private static void PopulateTypeSpecificFields(DialogueNode node, string[] values)
    {
        if (node.type == DialogueType.Choice)
        {
            node.correctChoiceIndex = 0;

            for (int j = 0; j < 3; j++)
            {
                int choiceTextIndex = 5 + j * 2;
                int choiceNextIndex = 6 + j * 2;

                if (choiceTextIndex < values.Length
                    && choiceNextIndex < values.Length
                    && !string.IsNullOrEmpty(values[choiceTextIndex]))
                {
                    node.choiceTexts[j] = CsvUtility.CleanString(values[choiceTextIndex]);
                    node.choiceNextIDs[j] = int.Parse(values[choiceNextIndex]);

                    // 다음 ID가 임계값 미만이면 정답 선택지
                    if (node.choiceNextIDs[j] < DIALOGUE_THRESHOLD_ID)
                    {
                        node.correctChoiceIndex = j;
                    }
                }
            }
        }
        else if (node.type == DialogueType.NumberChoice)
        {
            node.minValue = int.Parse(CsvUtility.CleanString(values[5]));
            node.maxValue = int.Parse(CsvUtility.CleanString(values[7]));
            node.choiceNextIDs[0] = int.Parse(values[6]);
            node.choiceNextIDs[1] = int.Parse(values[8]);
        }
        else if (node.type == DialogueType.Dialogue
                 || node.type == DialogueType.Advice
                 || node.type == DialogueType.CutScene)
        {
            node.nextDialogueID = node.dialogueID + 1;
        }
        // Success, GameOver, None: 추가 필드 없음
    }
}

/// <summary>대사 CSV 파싱 치명적 실패 시 예외.</summary>
public class DialogueDataParseException : Exception
{
    public DialogueDataParseException(string message) : base(message) { }
}