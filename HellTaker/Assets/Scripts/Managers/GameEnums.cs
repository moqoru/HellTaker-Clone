/// <summary>
/// 게임의 전체 상태를 나타냄
/// </summary>
public enum GameState
{
    None = -1,
    Playing,
    UI, // 모든 UI 상호작용
    Transition,
    // Paused, // 필요시 UIType 안으로 편입
}

/// <summary>
/// UI 상호작용 타입 설정
/// </summary>
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
