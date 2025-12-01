using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("방향키 입력 감도 설정")]
    [Tooltip("방향키 입력 감도 (0.5 권장)")]
    public float inputThreshold = 0.5f;

    // TODO : 인트로 대화씬 완성시 UI 상태로 시작하도록 변경
    private GameState currentState = GameState.Playing;
    private UIType currentUIType = UIType.Dialogue;
    private bool readyToMove = true;
    private bool waitingForInputRelease = false;

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

    }

    // Update is called once per frame
    private void Update()
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

        if (currentState != newState)
        {
            // state가 바뀌는 상황이라면 입력 초기화 대기
            waitingForInputRelease = true;

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
        // 같은 UI State라면 UIType을 한번 더 확인
        else if (newState == GameState.UI)
        {
            if (uiType == UIType.None)
            {
                Debug.LogError("[InputManager] UI 상태에서 UIType이 None일 수 없습니다.");
                uiType = UIType.Dialogue;
            }

            if (currentUIType != uiType)
            {
                // uiType가 바뀌는 상황이라면 입력 초기화 대기
                waitingForInputRelease = true;

                currentUIType = uiType;
            }
        }

    }

    public GameState GetState() => currentState;
    public UIType GetUIType() => currentUIType;

    private void HandlePlayingInput()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 입력 초기화 대기 중이면 모든 키가 떨어질 때까지 대기
        if (waitingForInputRelease)
        {
            if (IsAnyKeyPressed(moveInput))
            {
                return;
            }
            else
            {
                // 모든 키가 떨어졌으면 제한 해제
                waitingForInputRelease = false;
                readyToMove = true;
                Debug.Log("[InputManager] Playing 입력 초기화 완료");
            }
        }

        if (moveInput.sqrMagnitude > inputThreshold * inputThreshold)
        {
            if (readyToMove)
            {
                readyToMove = false;

                // 방향 정규화 (대각 입력 방지)
                Vector2Int direction = NormalizeDirection(moveInput);

                if (Player.Instance != null)
                {
                    Player.Instance.TryMove(direction);
                }
                else
                {
                    Debug.LogWarning("[InputManager] Player.Instance가 null입니다!");
                }
            }
        }
        else
        {
            readyToMove = true;
        }

        // TODO: 게임패드 LB, RB 키를 키보드 L,R키로 설정
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
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 입력 초기화 대기 중이면 모든 키가 떨어질 때까지 대기
        if (waitingForInputRelease)
        {
            if (IsAnyKeyPressed(moveInput))
            {
                return;
            }
            else
            {
                // 모든 키가 떨어졌으면 제한 해제
                waitingForInputRelease = false;
                readyToMove = true;
                Debug.Log("[InputManager] UI 입력 초기화 완료");
            }
        }

        Vector2Int direction = NormalizeDirection(moveInput);

        if (moveInput.sqrMagnitude > inputThreshold * inputThreshold)
        {
            if (readyToMove)
            {
                readyToMove = false;
                HandleDirectionInput(direction);
            }
        }
        else
        {
            readyToMove = true;
        }

        // TODO: 게임패드에서의 A 버튼 처리 추가 필요
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleConfirmInput();
        }

        // TODO: Esc 키와 게임패드에서의 B버튼 (또는 메뉴 버튼 처리 필요)
    }

    /** UI 내에서 방향키 입력 처리 */
    private void HandleDirectionInput(Vector2Int direction)
    {
        switch (currentUIType)
        {
            case UIType.Dialogue:
            case UIType.Advice:
            case UIType.GameOver:
                // 방향키 무시
                break;

            case UIType.Choice:
                if (direction.y > 0)
                {
                    // DialogueManager.Instance?.SelectPreviousChoice();
                }
                else if (direction.y < 0)
                {
                    // DialogueManager.Instance?.SelectNextChoice();
                }
                break;

            case UIType.StageSelect:
                if (direction.x < 0)
                {
                    // StageSelectManager.Instance?.SelectPreviousStage();
                }
                else if (direction.x > 0)
                {
                    // StageSelectManager.Instance?.SelectNextStage();
                }
                break;
        }
    }

    /** UI 내에서 확인 키 입력 처리 */
    private void HandleConfirmInput()
    {
        switch (currentUIType)
        {
            case UIType.Dialogue:
            case UIType.Advice:
                // DialogueManager.Instance?.AdvanceDialogue();
                break;
            case UIType.GameOver:
                SetState(GameState.Transition);
                GameManager.Instance.RestartStage();
                break;

            case UIType.Choice:
                // DialogueManager.Instance?.ConfirmChoice();
                break;

            case UIType.StageSelect:
                // StageSelectManager.Instance?.ConfirmStage();
                break;
        }
    }

    private bool IsAnyKeyPressed(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude > inputThreshold * inputThreshold)
        {
            return true;
        }

        // TODO: 게임패드 키 대응 설정 필요
        if (Input.GetKey(KeyCode.R)
            || Input.GetKey(KeyCode.L)
            || Input.GetKey(KeyCode.Return)
            || Input.GetKey(KeyCode.Escape))
        {
            return true;
        }

        return false;
    }
}
