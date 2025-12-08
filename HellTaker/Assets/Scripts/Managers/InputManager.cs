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

        // R키 or RB버튼: 재시작
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            SetState(GameState.Transition);
            GameManager.Instance.RestartStage();
        }

        // L키 or LB버튼: 인생 조언
        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            SetState(GameState.UI, UIType.Advice);
            DialogueManager.Instance.StartAdvice(GameManager.Instance.GetCurrentStage());
        }
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
                return; // 그 다음 프레임에서 다시 처리하기.
            }
        }

        // Dialogue/Advice UI 처리
        if (currentUIType == UIType.Dialogue || currentUIType == UIType.Advice)
        {
            if (DialogueManager.Instance.IsNumberChoice)
            {
                // 숫자 선택지 - 좌우 키로 증감
                if (Mathf.Abs(moveInput.x) > inputThreshold && readyToMove)
                {
                    DialogueManager.Instance.ChangeNumberValue(moveInput.x > 0 ? 1 : -1);
                    readyToMove = false;
                }

                // 스틱을 중립으로 놓아야 다시 이동 가능
                if (Mathf.Abs(moveInput.x) <= inputThreshold && !readyToMove)
                {
                    readyToMove = true;
                }

                // Enter 또는 A버튼으로 확정
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    DialogueManager.Instance.SelectNumberChoice();
                }
            }
            else if (DialogueManager.Instance.IsShowingChoice)
            {
                // 일반 선택지 - 상하 키로 이동
                if (Mathf.Abs(moveInput.y) > inputThreshold && readyToMove)
                {
                    DialogueManager.Instance.MoveChoiceSelection(moveInput.y > 0 ? -1 : 1);
                    readyToMove = false;
                }

                // 스틱을 중립으로 놓아야 다시 이동 가능
                if (Mathf.Abs(moveInput.y) <= inputThreshold && !readyToMove)
                {
                    readyToMove = true;
                }

                // Enter 또는 A버튼으로 확정
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    DialogueManager.Instance.SelectChoice();
                }
            }
            else
            {
                // 일반 대사를 다음으로 진행
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    DialogueManager.Instance.AdvanceDialogue();
                }
            }
        }
        else if (currentUIType == UIType.StageSelect)
        {
            if (moveInput.sqrMagnitude > inputThreshold * inputThreshold && readyToMove)
            {
                readyToMove = false;
                Vector2Int direction = NormalizeDirection(moveInput);

                if (direction.x < 0)
                {
                    // StageSelectManager.Instance?.SelectPreviousStage();
                }
                else if (direction.x > 0)
                {
                    // StageSelectManager.Instance?.SelectNextStage();
                }
            }

            if (moveInput.sqrMagnitude <= inputThreshold * inputThreshold && !readyToMove)
            {
                readyToMove = true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                // StageSelectManager.Instance?.ConfirmStage();
            }
        }
        else if (currentUIType == UIType.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                GameManager.Instance.RestartStage();
            }
        }
        // TODO: 일시정지 버튼 처리 필요
    }

    private bool IsAnyKeyPressed(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude > inputThreshold * inputThreshold)
        {
            return true;
        }

        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0) // A
            || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.JoystickButton1) // B
            || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.JoystickButton5) // RB
            || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.JoystickButton4)) // LB
        {
            return true;
        }

        return false;
    }
}
