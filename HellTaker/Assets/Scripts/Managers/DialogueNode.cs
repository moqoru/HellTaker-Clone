[System.Serializable]
public class DialogueNode
{
    public int dialogueID;
    public DialogueType type;
    public string characterImagePath;
    public string characterName;
    public string text;

    // 선택지 데이터 (Choice, NumberChoice)
    public string[] choiceTexts = new string[3];
    public int[] choiceNextIDs = new int[3];
    public int correctChoiceIndex;

    // NumberChoice 전용
    public int minValue;
    public int maxValue;

    // 다음 대사 ID(Dialouge, Advice)
    public int nextDialogueID;
}

public enum DialogueType
{
    None = -1,
    Dialogue,
    Choice,
    NumberChoice,
    Success,
    GameOver,
    Advice,
}