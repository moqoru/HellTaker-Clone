using UnityEngine;

public enum GameState
{
    None = -1,
    Playing,
    UI, // 모든 UI 상호작용
    Transition,
    // Paused, // 필요시 UIType 안으로 편입
}

public enum UIType
{
    None = -1,
    Dialogue, // 일반 대화, 사실상 기본 상태 (Enter만)
    CutScene, // Dialogue보다 큰 이미지 + 대화창 (Enter만)
    Advice, // Dialogue와 같지만 트랜지션 없음 (Enter만)
    GameOver, // 선택지 오답시 게임 오버 (Enter로 재시작)
    Choice, // 선택지 (상하 + Enter)
    StageSelect, // 스테이지 선택 (좌우 + Enter)
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {  get; private set; }

    [Header("방향키 입력 감도 설정")]
    [Tooltip("방향키 입력 감도 (0.5 권장)")]
    public float inputThreshold = 0.5f;

    // TODO : 인트로 대화씬 완성시 UI 상태로 시작하도록 변경
    private GameState currentState = GameState.Playing;
    private UIType currentUIType = UIType.Dialogue;
    private bool readyToMove = true;

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
    }

    void Start()
    {
        readyToMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GameState.Playing:
                HandlePlayingInput();
                break;
            case GameState.UI:
                HandleUIInput();
                break;
            case GameState.Transition:
                // 입력 차단
                break;
            // case GameState.Paused:
        }
    }

    public void SetState(GameState newState, UIType uiType = UIType.None)
    {
        Debug.Log($"[InputManager] {currentState} -> {newState}" +
            (newState == GameState.UI ? $" ({uiType})" : ""));

        currentState = newState;

        if (newState == GameState.UI)
        {
            if (uiType == UIType.None)
            {
                Debug.LogError("[InputManager] UI 상태에서 UIType이 None일 수 없습니다.");
                currentUIType = UIType.Dialogue;
            }
            else
            {
                currentUIType = uiType;
            }
        }
        else
        {
            currentUIType = UIType.None;
        }
        
    }

    public GameState GetState() => currentState;
    public UIType GetUIType() => currentUIType;
    
    private void HandlePlayingInput()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (moveInput.sqrMagnitude > inputThreshold * inputThreshold)
        {
            if (readyToMove)
            {
                readyToMove = false;

                // 방향 정규화 (대각 입력 방지)
                Vector2Int direction = NormalizeDirection(moveInput);
                Player.Instance?.TryMove(direction);
            }
        }
        else
        {
            readyToMove = true;
        }

        // R키: 재시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetState(GameState.Transition);
            GameManager.Instance.RestartStage();
        }

        // L키: 인생 조언(추후 구현)
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //    SetState(GameState.UI, UIType.Advice);
        //    DialogueManager.Instance.ShowAdvice();
        // }
    }

    /** 입력을 4방향 중 하나로 정규화 */
    private Vector2Int NormalizeDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return new Vector2Int(input.x > 0 ? 1 : -1, 0);
        }
        else
        {
            return new Vector2Int(0, input.y > 0 ? 1 : -1);
        }
    }

    private void HandleUIInput()
    {
        switch (currentUIType)
        {
            case UIType.Dialogue:
            case UIType.Advice:
            case UIType.GameOver:
                HandleDialogueInput();
                break;

            case UIType.Choice:
                HandleChoiceInput();
                break;

            case UIType.StageSelect:
                HandleStageSelectInput();
                break;
        }
    }

    private void HandleDialogueInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // TODO: DialogueManager 구현 후 연결
            // DialogueManager.Instance?.AdvanceDialogue();

            // 임시: 게임 오버 상태면 재시작
            if (currentUIType == UIType.GameOver)
            {
                SetState(GameState.Transition);
                GameManager.Instance.RestartStage();
            }
        }
    }

    private void HandleChoiceInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // DialogueManager.Instance?.SelectPreviousChoice();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // DialogueManager.Instance?.SelectNextChoice();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // DialogueManager.Instance?.ConfirmChoice();
        }
    }

    private void HandleStageSelectInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // StageSelectManager.Instance?.SelectPreviousStage();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // StageSelectManager.Instance?.SelectNextStage();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // StageSelectManager.Instance?.ConfirmStage();
        }
    }

}
