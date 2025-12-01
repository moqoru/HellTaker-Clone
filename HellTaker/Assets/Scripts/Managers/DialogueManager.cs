using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public CanvasGroup dialoguePanel; // Alpha 제어용
    public Image characterImage;
    public Text characterNameText;
    public Text dialogueText;
    public CanvasGroup choicePanel; // 선택지 Alpha 제어용
    public List<Image> choiceBackgrounds; // 선택지 배경 (하이라이트용)
    public List<Text> choiceTexts;

    [Header("UI Colors")]
    public Color normalChoiceColor = Color.white;
    public Color hightlightedChoiceColor = Color.yellow;

    [Header("Character Sprites")]
    private Dictionary<string, Sprite> characterSprites;

    private Dictionary<int, DialogueNode> currentDialogueData;
    private int currentDialogueID;
    private int currentChoiceIndex = 0;
    private int activeChoiceCount = 0;

    // NumberChoice 전용
    private bool isNumberChoice = false;
    private int currentNumberValue;
    private int numberChoiceMin;
    private int numberChoiceMax;
    private DialogueNode numberChoiceNode;

    // 콜백
    public System.Action OnDialogueEnd;
    public System.Action<string> OnWrongChoice; // 게임오버 메시지 전달

    // 프로퍼티
    public bool IsShowingChoice { get; private set; }
    public bool IsNumberChoice => isNumberChoice;
    public bool IsActive { get; private set; }

    void Awake()
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

        // 초기화
        dialoguePanel.alpha = 0;
        choicePanel.alpha = 0;
        IsActive = false;
        IsShowingChoice = false;

        // 캐릭터 스프라이트 로드
        LoadCharacterSprites();
    }

    void LoadCharacterSprites()
    {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Scene/Characters");

        foreach (Sprite sprite in sprites)
        {
            characterSprites[sprite.name] = sprite;
        }

        Debug.Log($"[DialogueManager] {characterSprites.Count}개의 스트라이트를 로드했습니다.");
    }

    public void StartDialogue(int stageNumber)
    {
        // csv 파일 로드
        LoadDialogueData($"Stage{stageNumber}");
        
        if (currentDialogueData == null || currentDialogueData.Count == 0)
        {
            Debug.LogError($"[DialogueManager] {stageNumber}스테이지의 대화 데이터를 불러오지 못했습니다.");
            return;
        }

        // 첫 번째 대사 표시 (항상 시작 ID는 1번)
        currentDialogueID = 1;
        IsActive = true;

        // UI 활성화
        StartCoroutine(FadeIn(dialoguePanel));

        // 첫 대사 표시
        ShowDialogue(currentDialogueID);

        // InputManager에 상태 알림
        InputManager.Instance.SetState(GameState.UI, UIType.Dialogue);
    }

    public void StartAdvice(int stageNumber)
    {
        // 힌트 대사 csv에서 로드
        LoadDialogueData($"Stage{stageNumber}");

        if (currentDialogueData == null || currentDialogueData.Count == 0)
        {
            Debug.LogError($"[DialogueManager] {stageNumber}스테이지의 대화 데이터를 불러오지 못했습니다.");
            return;
        }

        // 인생 조언의 대사 ID는 201부터 시작
        currentDialogueID = 201;
        IsActive = true;

        // UI 활성화
        StartCoroutine(FadeIn(dialoguePanel));

        // 첫 대사 표시
        ShowDialogue(currentDialogueID);

        // InputManager에 상태 알림
        InputManager.Instance.SetState(GameState.UI, UIType.Advice);
    }

    void LoadDialogueData(string csvFileName)
    {
        currentDialogueData = new Dictionary<int, DialogueNode>();

        TextAsset csvFile = Resources.Load<TextAsset>("Dialogues/" + csvFileName);
        if (csvFile == null)
        {
            Debug.LogError($"[DialogueManager] {csvFileName} 파일을 불러오지 못했습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        // 0번째 줄은 헤더
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = ParseCSVLine(lines[i]);
            if (values.Length < 5) continue; // 필드의 최솟값 이상 채워져 있는지 체크

            DialogueNode node = new DialogueNode
            {
                dialogueID = int.Parse(values[0]),
                type = (DialogueType)System.Enum.Parse(typeof(DialogueType), values[1]),
                characterImagePath = CleanString(values[2]),
                characterName = CleanString(values[3]),
                text = CleanString(values[4]),
            };

            // Choice 타입일 때 선택지 데이터 파싱
            if (node.type == DialogueType.Choice)
            {
                // 정답 선택지 인덱스 (첫번째를 정답으로 초기 설정)
                node.correctChoiceIndex = 0;

                int validChoices = 0;
                for (int j = 0; j < 3; j++)
                {
                    int choiceTextIndex = 5 + j * 2;
                    int choiceNextIndex = 6 + j * 2;

                    if (choiceTextIndex < values.Length
                        && choiceNextIndex < values.Length
                        && !string.IsNullOrEmpty(values[choiceTextIndex]))
                    {
                        node.choiceTexts[j] = CleanString(values[choiceTextIndex]);
                        node.choiceNextIDs[j] = int.Parse(values[choiceNextIndex]);
                        validChoices++;

                        // 다음 ID 번호가 한 자릿수이면 일반 대화 => 정답 선택지 인덱스로 설정
                        if (node.choiceNextIDs[j] < 100)
                        {
                            node.correctChoiceIndex = j;
                        }
                    }
                }
            }
            else if (node.type == DialogueType.NumberChoice)
            {
                // NumberChoice: Choice1에 최솟값, 2에 최댓값
                node.minValue = int.Parse(CleanString(values[5]));
                node.maxValue = int.Parse(CleanString(values[7]));
                node.choiceNextIDs[0] = int.Parse(values[6]); // 1 ~ 9 선택
                node.choiceNextIDs[1] = int.Parse(values[8]); // 10 선택
            }
            else if (node.type == DialogueType.Dialogue || node.type == DialogueType.Advice)
            {
                // 일반적인 경우 다음 대사 ID는 순차적으로 증가
                node.nextDialogueID = node.dialogueID + 1;
            }

            currentDialogueData.Add(node.dialogueID, node);
        }

        Debug.Log($"[DialogueManager] {csvFileName} 파일의 {currentDialogueData.Count} 노드의 정보를 로드했습니다.");
    }



}
