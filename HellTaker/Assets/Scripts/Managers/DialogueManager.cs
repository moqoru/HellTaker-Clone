using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public CanvasGroup dialoguePanel;
    public Image characterBackGroundImage;
    public Image characterImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public CanvasGroup choicePanel;
    public List<Image> choiceBackgrounds;
    public List<TextMeshProUGUI> choiceTexts;
    public CanvasGroup successPanel;
    public Image successImage;
    public int normalFontSize = 29;
    public int highlightedFontSize = 48;

    [Header("Choice Sprites")]
    public Sprite selectedChoiceSprite;
    public Sprite unselectedChoiceSprite;

    [Header("Character Sprites")]
    private Dictionary<string, Sprite> characterSprites;

    // ID 구분 번호
    private const int DIALOGUE_START_ID = 1;
    private const int DIALOGUE_THRESHOLD_ID = 100;
    private const int ADVICE_START_ID = 201;

    private Dictionary<int, DialogueNode> currentDialogueData;
    private int currentDialogueID;
    private int currentChoiceIndex = 0;
    private int activeChoiceCount = 0;

    // NumberChoice 전용
    private int currentNumberValue = 0;
    private int numberChoiceMin = 0;
    private int numberChoiceMax = 0;
    private DialogueNode numberChoiceNode = null;

    // 콜백
    public System.Action OnDialogueEnd;
    public System.Action<string> OnWrongChoice; // 게임오버 메시지 전달

    // 프로퍼티
    public bool IsShowingChoice { get; private set; }
    public bool IsNumberChoice { get; private set; } = false;
    public bool IsActive { get; private set; }

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

        // 초기화
        dialoguePanel.alpha = 0;
        choicePanel.alpha = 0;
        IsActive = false;
        IsShowingChoice = false;

        // 캐릭터 스프라이트 로드
        LoadCharacterSprites();
    }

    private void LoadCharacterSprites()
    {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Scene/Characters");

        foreach (Sprite sprite in sprites)
        {
            characterSprites[sprite.name] = sprite;
        }
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

        currentDialogueID = DIALOGUE_START_ID;
        IsActive = true;

        StartCoroutine(FadeIn(dialoguePanel));

        ShowDialogue(currentDialogueID);

        InputManager.Instance.SetState(GameState.UI, UIType.Dialogue);
        AudioManager.Instance.PlaySFX(SFXType.DialogueOpen);
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
        currentDialogueID = ADVICE_START_ID;
        IsActive = true;

        StartCoroutine(FadeIn(dialoguePanel));

        ShowDialogue(currentDialogueID);

        InputManager.Instance.SetState(GameState.UI, UIType.Advice);
        AudioManager.Instance.PlaySFX(SFXType.DialogueOpen);
    }

    private void LoadDialogueData(string csvFileName)
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

            if (!int.TryParse(values[0], out int id))
            {
                Debug.LogWarning($"[DialogueManager] csv {i}번째 줄: ID 파싱 실패");
                continue;
            }

            if (!System.Enum.TryParse(values[1], out DialogueType type))
            {
                Debug.LogWarning($"[DialogueManager] csv {i}번째 줄: 타입 파싱 실패");
                continue;
            }

            DialogueNode node = new DialogueNode
            {
                dialogueID = id,
                type = type,
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
                        if (node.choiceNextIDs[j] < DIALOGUE_THRESHOLD_ID)
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

    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            // ',' 기호가 칼럼 구분자인지, 아니면 따옴표 안에 있는 문장 부호인지 구분하여 저장
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result.ToArray();
    }

    /// <summary>
    /// 문자열에서 따옴표가 3쌍씩 생기는 문제와 \n 기호가 실제와 다른 것을 정상화
    /// </summary>
    private string CleanString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";

        // 앞뒤 따옴표 제거 (따옴표 3쌍씩 있는 경우도 포함)
        str = str.Trim();
        if (str.StartsWith("\"\"\"")) str = str.Substring(3);
        if (str.EndsWith("\"\"\"")) str = str.Substring(0, str.Length - 3);
        if (str.StartsWith("\"")) str = str.Substring(1);
        if (str.EndsWith("\"")) str = str.Substring(0, str.Length - 1);

        // \n을 실제 줄바꿈으로 변경
        str = str.Replace("\\n", "\n");

        // 앞 뒤 공백 제거하여 깔끔하게 만들기
        return str.Trim();
    }

    private void ShowDialogue(int dialogueID)
    {
        if (!currentDialogueData.ContainsKey(dialogueID))
        {
            Debug.LogError($"[DialogueManager] 현재 스테이지의 {dialogueID}번 대사를 불러오지 못했습니다.");
            EndDialogue(false);
            return;
        }

        DialogueNode node = currentDialogueData[dialogueID];

        // 캐릭터 이미지 설정
        if (!string.IsNullOrEmpty(node.characterImagePath))
        {
            string spriteName = System.IO.Path.GetFileNameWithoutExtension(node.characterImagePath);

            if (characterSprites.ContainsKey(spriteName))
            {
                characterImage.sprite = characterSprites[spriteName];

                characterImage.SetNativeSize();

                characterImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] 캐릭터 이미지를 불러오지 못했습니다: {spriteName}");
                characterImage.enabled = false;
            }
        }
        else
        {
            characterImage.enabled = false;
        }

        // 캐릭터 이름 설정
        if (!string.IsNullOrEmpty(node.characterName))
        {
            characterNameText.text = $"• {node.characterName} •";
            characterNameText.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            characterNameText.transform.parent.gameObject.SetActive(false);
        }

        // 대사 텍스트 설정
        dialogueText.text = node.text;

        switch(node.type)
        {
            case DialogueType.Choice:
                ShowChoices(node);
                break;
            case DialogueType.NumberChoice:
                ShowNumberChoice(node);
                break;
            case DialogueType.GameOver:
                EndDialogue(false);
                OnWrongChoice?.Invoke(node.text);
                break;
            case DialogueType.Success:
                // TODO : Glorius Success와 우효해골 이미지 나오는 스테이지 대응
                successPanel.alpha = 1;
                break;
            default: // Dialogue, Advice
                IsShowingChoice = false;
                IsNumberChoice = false;
                choicePanel.alpha = 0;
                break;
        }

    }

    private void ShowChoices(DialogueNode node)
    {
        IsShowingChoice = true;
        IsNumberChoice = false;
        currentChoiceIndex = 0;

        // 선택지 개수 확인
        activeChoiceCount = 0;
        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(node.choiceTexts[i]))
            {
                choiceTexts[i].text = node.choiceTexts[i];
                choiceTexts[i].fontSize = normalFontSize;
                choiceBackgrounds[i].gameObject.SetActive(true);
                activeChoiceCount++;
            }
            else
            {
                choiceBackgrounds[i].gameObject.SetActive(false);
            }
        }

        // 선택지 UI 활성화
        StartCoroutine(FadeIn(choicePanel));

        // 첫 번째 선택지 하이라이트
        UpdateChoiceHighlight();
    }

    /** NumberChoice 시스템 (현재 미사용, 향후 기능 구현시 자동 작동) */
    private void ShowNumberChoice(DialogueNode node)
    {
        IsShowingChoice = true;
        IsNumberChoice = true;

        // 숫자 선택 초기화
        numberChoiceMin = node.minValue;
        numberChoiceMax = node.maxValue;
        // TODO: 이전에 한 숫자 선택 기억하기, 구조체나 클래스로 묶어서 관리해보기
        currentNumberValue = numberChoiceMin;
        numberChoiceNode = node;

        // 첫 번째 선택지만 사용 (숫자 표시)
        choiceBackgrounds[0].gameObject.SetActive(true);
        choiceBackgrounds[1].gameObject.SetActive(false);
        choiceBackgrounds[2].gameObject.SetActive(false);

        // UI 업데이트
        UpdateNumberChoiceDisplay();

        StartCoroutine(FadeIn(choicePanel));
    }

    private void UpdateNumberChoiceDisplay()
    {
        choiceTexts[0].text = currentNumberValue.ToString();
        choiceTexts[0].fontSize = highlightedFontSize;
        // choiceBackgrounds[0].color = highlightedChoiceColor;
    }

    public void AdvanceDialogue()
    {
        if (IsShowingChoice) return; // 선택지가 보일 때는 넘기기 

        DialogueNode currentNode = currentDialogueData[currentDialogueID];

        if (currentNode.type == DialogueType.Dialogue || currentNode.type == DialogueType.Advice)
        {
            // 게임 클리어가 아닐 경우에는 모두 넘기기 효과음 재생
            AudioManager.Instance.PlaySFX(SFXType.DialogueAdvance);

            // 다음 대사가 있는지 확인
            if (currentDialogueData.ContainsKey(currentNode.nextDialogueID))
            {
                // 다음 대사의 타입 확인
                DialogueNode nextNode = currentDialogueData[currentNode.nextDialogueID];

                // Advice에서 Advice가 아닌 대사로 넘어가면 인생 조언 종료
                // TODO : NumberChoice 완성 후, Advice에서 NumberChoice로 넘어갈 때는 자동으로 넘어가도록 처리
                if (currentNode.type == DialogueType.Advice && nextNode.type != DialogueType.Advice)
                {
                    EndDialogue(true); // 인생 조언 종료 후 게임으로 복귀
                    return;
                }

                currentDialogueID = currentNode.nextDialogueID;
                ShowDialogue(currentDialogueID);
            }

            else
            {
                if (currentNode.type == DialogueType.Advice)
                {
                    EndDialogue(true); // 인생 조언 종료 - 아직 게임 클리어 아님
                }
                else
                {
                    Debug.LogWarning("[DialogueManager] 다음 대사를 찾을 수 없습니다.");
                    EndDialogue(false); // 일반 대화 종료 (게임 클리어 처리?)
                }
                return;
            }
        }
        else if (currentNode.type == DialogueType.Success)
        {
            EndDialogue(false);
            OnDialogueEnd?.Invoke();
            return;
        }
        else
        {
            Debug.LogError("[DialogueManager] 다음 대사로 넘어가지 못했습니다.");
            return;
        }
    }

    public void MoveChoiceSelection(int direction)
    {
        if (!IsShowingChoice || IsNumberChoice) return;

        AudioManager.Instance.PlaySFX(SFXType.DialogueSelect);

        currentChoiceIndex += direction;

        // 순환 가능한 커서 이동 처리
        if (currentChoiceIndex < 0)
            currentChoiceIndex = activeChoiceCount - 1;
        else if (currentChoiceIndex >= activeChoiceCount)
            currentChoiceIndex = 0;

        UpdateChoiceHighlight();
    }

    void UpdateChoiceHighlight()
    {
        for (int i = 0; i < activeChoiceCount; i++)
        {
            if (i == currentChoiceIndex)
            {
                choiceBackgrounds[i].sprite = selectedChoiceSprite;
                choiceBackgrounds[i].SetNativeSize();
            }
            else
            {
                choiceBackgrounds[i].sprite = unselectedChoiceSprite;
                choiceBackgrounds[i].SetNativeSize();
            }
        }
    }

    public void SelectChoice()
    {
        if (!IsShowingChoice || IsNumberChoice) return;

        DialogueNode currentNode = currentDialogueData[currentDialogueID];

        if (currentChoiceIndex == currentNode.correctChoiceIndex)
        {
            // 정답 선택 - 다음 대사로
            int nextID = currentNode.choiceNextIDs[currentChoiceIndex];
            currentDialogueID = nextID;

            AudioManager.Instance.PlaySFX(SFXType.DialogueSuccess);
        }
        else
        {
            // 오답 선택 - 게임오버 대사로
            int gameOverID = currentNode.choiceNextIDs[currentChoiceIndex];
            currentDialogueID = gameOverID;

            AudioManager.Instance.PlaySFX(SFXType.DialogueConfirm);
        }

        // 선택지 다시 숨기기
        IsShowingChoice = false;
        StartCoroutine(FadeOut(choicePanel));

        ShowDialogue(currentDialogueID);
    }

    /** NumberChoice 시스템 (현재 미사용, 향후 기능 구현시 자동 작동) */
    public void ChangeNumberValue(int delta)
    {
        if (!IsNumberChoice) return;

        AudioManager.Instance.PlaySFX(SFXType.DialogueSelect);

        currentNumberValue += delta;

        // min과 max 사이로 범위 제한
        if (currentNumberValue < numberChoiceMin)
            currentNumberValue = numberChoiceMin;
        else if (currentNumberValue > numberChoiceMax)
            currentNumberValue = numberChoiceMax;

        UpdateNumberChoiceDisplay();
    }

    /** NumberChoice 시스템 (현재 미사용, 향후 기능 구현시 자동 작동) */
    public void SelectNumberChoice()
    {
        if (!IsNumberChoice) return;

        AudioManager.Instance.PlaySFX(SFXType.DialogueConfirm);

        IsNumberChoice = false;
        IsShowingChoice = false;

        // 선택한 숫자에 따라 분기
        int nextID;
        if (currentNumberValue == numberChoiceMax) // 10일 때만 뒤쪽 분기로
        {
            nextID = numberChoiceNode.choiceNextIDs[1];
        }
        else
        {
            nextID = numberChoiceNode.choiceNextIDs[0];
        }

        currentDialogueID = nextID;
        StartCoroutine(FadeOut(choicePanel));
        ShowDialogue(currentDialogueID);
    }

    private void EndDialogue(bool returnToGame)
    {
        IsActive = false;
        IsShowingChoice = false;
        IsNumberChoice = false;

        // 선택지와 성공 이미지를 페이드 아웃 전에 먼저 숨기기
        // TODO : 트랜지션 화면이 '절반을 덮었을 때' 사라지도록 변경하기
        choicePanel.alpha = 0;
        successPanel.alpha = 0;

        // UI 비활성화
        StartCoroutine(FadeOut(dialoguePanel));
        // StartCoroutine(FadeOut(successPanel));
        // StartCoroutine(FadeOut(choicePanel));

        // 게임으로 복귀해야 할 경우 (인생 조언 종료 시)
        if (returnToGame)
        {
            InputManager.Instance.SetState(GameState.Playing);
        }
    }

    IEnumerator FadeIn(CanvasGroup group)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            yield return null;
        }

        group.alpha = 1;
    }

    IEnumerator FadeOut(CanvasGroup group)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            yield return null;
        }

        group.alpha = 0;
    }
}