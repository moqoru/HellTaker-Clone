 ![Unity Tests](https://github.com/moqoru/HellTaker-Clone/actions/workflows/unity-test.yml/badge.svg)

# Helltaker (헬테이커) 모작

- 그리드 기반 퍼즐 게임인 [Helltaker](https://store.steampowered.com/app/1289310/Helltaker/)의 Unity, C# 기반 모작입니다.
- 시연 영상 : [Youtube](https://youtu.be/DSWAkPyQHjs)
- 개발 인원 : 1명
- 제작 기간 : 3개월 (2025.10.21 - 2026.01.31)


# 핵심 기능

- 레벨 시스템
- 대화 시스템
- 그리드 이동 시스템
- 애니메이션 및 이펙트 연출
- 사운드 시스템

# 주요 시스템 상세

## 레벨 시스템

**레벨별 맵을 CSV 데이터로 관리하여 외부 데이터 수정만으로 구조를 변경할 수 있도록 설계했습니다.**

![레벨 CSV 예시](images/stage3_map_csv.png)

![레벨 CSV 적용 화면](images/stage3_map_ingame.gif)

**CSV 구조**

- **1번째 줄**: 이동 횟수, 표시 오프셋(x, y), 골 캐릭터 이름
  - 예시) `32, 0.5, 0, Cerberus` - 32턴, 오프셋(x: 0.5, y: 0), 케르베로스
- **2번째 줄 이후**: 타일 맵 데이터
  - `P`: 플레이어, `G`: 골, `#`: 벽, `B`: 블록, `M`: 몬스터
  - `T`: 일반 가시, `U`: 올라온 가시, `D`: 내려간 가시
  - `K`: 열쇠, `L`: 자물쇠, `.`: 빈 공간

**MapDataParser.cs**가 CSV를 파싱해 `ParsedMap` 구조체로 반환하고, **LevelManager.cs**가 이를 받아 그리드 좌표별로 해당하는 프리팹을 생성합니다.

## 대화 시스템

**대화 데이터를 CSV로 관리하며, 선택지 ID 범위로 대화 타입과 분기를 자동 판별합니다.**

![대화 CSV 예시](images/stage4_dialogue_csv.png)

![대화 CSV 적용 화면](images/stage4_dialogue_ingame.gif)

**CSV 구조**

- **DialogueID**: 대화 고유 번호 (\~99: 선택지와 정답 분기, 100\~199: 오답 분기, 200\~299: 힌트)
- **Type**: `Dialogue`(일반 대화), `Choice`(선택지), `Success`(클리어), `GameOver`(게임 오버), `Advice`(힌트)
- **CharacterImage, CharacterName, Text**: 캐릭터 이미지 경로, 이름, 대사
- **Choice1\~3, Choice1Next\~3Next**: 선택지 텍스트와 다음 대화 ID

**DialogueDataParser.cs**가 CSV를 파싱해 `DialogueNode` 딕셔너리로 변환하고, **DialogueManager.cs**가 이를 받아 상태 관리 및 분기 처리를 담당합니다.

## 그리드 이동 시스템

**GridManager.cs**에서 좌표별 오브젝트를 `Dictionary<Vector2Int, List<GameObject>>`로 관리하며, 이동 가능 여부를 판정하고 플레이어와 다른 오브젝트 간 상호작용을 중재합니다.

**주요 기능**

- **월드 좌표 <-> 그리드 좌표 변환**: `WorldToGrid(), GridToWorld()`
- **이동 가능 여부 검사**: `IsPositionBlocked()` - Wall 태그 포함시 이동 불가
- **페널티 타일 검사**: `IsPositionPunished()` - ThornNormal, ThornUp 태그 포함시 이동 횟수 페널티
- **밀 수 있는 오브젝트 확인**: `GetPushableAt()` - Block, Monster, LockBox 태그 여부 확인

## 애니메이션 및 이펙트 연출

### PlayerAnimator.cs

- Animator State Machine으로 Move/Kick 애니메이션 제어
- `IsAnimating` 프로퍼티로 애니메이션 재생 중 중복 입력 방지
- DOTween을 이용한 피격 이펙트 연출 (`FlashDamage()`)

### PlayerDeathAnimator.cs

- 스프라이트 시퀀스로 사망 애니메이션 구현
- 프레임별 스프라이트 배열을 `frameInterval` 간격으로 순차 재생
- 다른 오브젝트를 가릴 수 있게 월드 좌표를 스크린 좌표로 변환해 UI Canvas에 배치

### EffectManager.cs

- 이펙트 프리팹을 그리드 좌표 기반으로 생성
- `EffectType` enum으로 이펙트 종류 관리
- 그리드 좌표를 이펙트를 표시할 월드 좌표로 변환 후 생성 (`PlayEffectAtGrid()`)
- 두 그리드 사이 지점에 이펙트 생성 (`PlayEffectBetweenGrids()`)

## 사운드 시스템

**AudioManager.cs**에서 BGM과 SFX를 타입별로 관리합니다.

**주요 기능**

- **BGMType / SFXType enum**: 사운드를 타입별로 관리
- **볼륨 조절**: `SetBGMVolume()`, `SetSFXVolume()`으로 런타임에도 조정 가능
- **BGM과 게임 상태 연동**: Opening -> Game -> Ending 상태 전환시 자동으로 BGM 변경
- **액션별 SFX 재생**: 이동, 킥, 피격, 대화 등 각 액션마다 적절한 효과음 재생

# TC 문서와 자동화 테스트의 연결

본 프로젝트는 두 층위의 검증을 운영합니다.

- [**수동 TC 문서 (117개)**](https://naver.me/5LQHHwiI): 사용자 관점의 외부 행동 검증 (애니메이션, 사운드, UI 등 시각/청각 영역 포함)
- **자동화 단위 테스트 (49개)**: 코드 단위 로직 검증 (좌표 계산, 데이터 파싱, 상태 판정)

자동화 테스트는 크게 세 영역으로 나뉩니다.

### Type A. 수동 TC를 직접 대체하는 테스트

수동 TC 중 퍼즐 파트의 일부 케이스를 코드 레벨에서 자동 검증합니다.
(아래 항목은 모두 `GridManagerTests` 클래스의 메서드를 가리킵니다.)

| TC ID | 수동 TC 항목 | 자동화 테스트 |
|---|---|---|
| PZ-003 | 몬스터 | `IsPositionBlocked_WallTag_ReturnsTrue`<br>`MoveObject_UpdatesDictionary`<br>`GetPushableAt_PushableTags_ReturnsObject`<br>`UnregisterObject_RemovesFromGrid` |
| PZ-004 | 블록 | `IsPositionBlocked_WallTag_ReturnsTrue`<br>`MoveObject_UpdatesDictionary`<br>`GetPushableAt_PushableTags_ReturnsObject` |
| PZ-005 | 이동 불가 위치 | `IsPositionBlocked_WallTag_ReturnsTrue`<br>`IsPositionBlocked_EmptyPos_ReturnsFalse`<br>`GetPushableAt_NonPushableTag_ReturnsNull` |
| PZ-012 | 일반 가시 | `IsPositionPunished_ThornNormalTag_ReturnsTrue` |

### Type B. 수동 TC의 핵심 판정 로직만 부분 검증하는 테스트

수동 TC가 검증하는 행동의 **핵심 판정 로직**을 코드 레벨에서 자동 검증합니다. TC 전체 시나리오를 대체하지는 않으며, 해당 로직이 회귀로 깨지지 않았음을 보장합니다.
(아래 항목은 모두 `GridManagerTests` 클래스의 메서드를 가리킵니다.)

| TC ID | 수동 TC 항목 | 자동화 테스트 | 검증 층위 |
|---|---|---|---|
| PZ-006~010 | 스테이지 클리어 혹은 재시작 | `ClearGrid_RemovesAllObjects` | 맵 그리드 초기화 |
| PZ-009 | 퍼즐 완료 | `WorldToGrid_ZeroBase_ReturnsCorrectGrid`<br>`GridToWorld_ZeroBase_ReturnsCorrectGrid`<br>`IsPositionBlocked_GoalTag_ReturnsTrue`<br>`GetObjectWithTagAt_ReturnsCorrectObject` | 골 지점 인접 판정 |
| PZ-013 | 자물쇠(열쇠 획득 전) | `GetPushableAt_PushableTags_ReturnsObject` | 자물쇠 태그 구분 |
| PZ-014 | 열쇠와 자물쇠 | `GetObjectWithTagAt_ReturnsCorrectObject`<br>`UnregisterObject_RemovesFromGrid` | 열쇠 태그 구분<br>자물쇠 제거 |
| PZ-015 | 토글 가시 | `IsPositionBlocked_ThornUpTag_ReturnsTrue` | 가시가 올라왔을 때 동작 |
| PZ-026~029, 032 | 위치 구분 이펙트 | `WorldToGrid_ZeroBase_ReturnsCorrectGrid`<br>`GridToWorld_ZeroBase_ReturnsCorrectGrid` | 이펙트 위치의 정확성 |

### Type C. 데이터 무결성을 보장하는 테스트 (TC 직접 매칭 없음)

대사/맵 CSV 파싱 로직을 검증합니다. 수동 TC에서 검증하는 결과물(화면에 표시되는 대사, 로드된 맵)은 **올바르게 파싱된 데이터**에서 출발한다는 전제에서 성립하고, 자동화 테스트는 그 **파싱 단계 자체의 정확성**을 보장합니다.

- `MapDataParserTests` (15개): 맵 CSV의 메타데이터 및 타일 데이터 파싱
- `DialogueDataParserTests` (12개): 대사 노드 타입별 파싱 및 분기 처리
- `CsvUtilityTests` (7개): CSV 공통 로직 (따옴표/콤마/줄바꿈 처리)

이 영역은 수동 TC와 **직접 1:1 매핑되지 않습니다.** TC는 화면 표시 결과를 검증하고, 자동화 테스트는 **표시 직전 단계**의 데이터 정확성을 검증합니다. 두 검증은 서로의 사각지대를 보완합니다.

### 자동화 미커버 영역

퍼즐 (40개), 트랜지션 (24개), 컷신/대화의 시각/청각 영역 (49개): 시각/청각 판정이 필요하므로 수동 TC 영역에 유지됩니다.

# 기술 스택

- Unity 6000.2.8f1
- DOTween 1.2.790
- CSV 파싱 (레벨 / 대화 시스템 관리)

# 파일 구조

- Scripts/
  - Managers/    - 게임 전체 시스템 관리
    - GameManager.cs
    - LevelManager.cs
    - GridManager.cs
    - DialogueManager.cs
    - AudioManager.cs
    - EffectManager.cs
    - InputManager.cs
  - Animator/    - 연출 및 애니메이션 제어
    - PlayerAnimator.cs
    - PlayerDeathAnimator.cs
  - Utils/     - 공통 유틸리티
    - CsvUtility.cs
  - Parsers/    - CSV 데이터 파싱 (단위 테스트 대상)
    - MapDataParser.cs
    - DialogueDataParser.cs
- Tests/    - 단위 테스트
  - GridManagerTests.cs
  - MapDataParserTests.cs
  - DialogueDataParserTests.cs
  - CsvUtilityTests.cs
- Resources/
  - Dialogues/    - 대화 데이터 (CSV)
    - Opening.csv
    - Stage1.csv ~ Stage7.csv
    - Ending.csv
  - Stages/    - 맵 데이터 (CSV) 
    - Stage1.csv ~ Stage7.csv

# 플레이 방법

## 게임 목표

- 각 레벨에서 주어진 행동 포인트 안에 골 캐릭터 주변 타일로 도달해야 합니다.
  - 블럭과 몬스터는 주변에서 걷어차 밀어낼 수 있으며, 몬스터는 벽 방향으로 걷어차 제거할 수 있습니다.
  - 가시에 찔리면 행동 포인트가 한 번에 2씩 줄어듭니다.
  - 열쇠 아이템을 얻은 뒤 자물쇠 위치로 이동하면 자물쇠가 사라집니다.
- 골 캐릭터 주변에 도착하면 대화 창이 열리며 선택지가 제시됩니다.
  - 맞는 선택지를 고르면 다음 스테이지로 넘어가며, 틀린 선택지를 고르면 그 레벨을 다시 시작합니다.
- 총 7개의 레벨을 마치면 게임이 완전히 클리어됩니다.

## 조작키 (키보드 / 게임패드)

- 이동, 선택 : 방향키 / L 스틱
- 넘기기, 확인 : Enter / A 버튼
- 퍼즐 스킵 : Esc / B 버튼
- 재시작 : R키 / RB 버튼
- 힌트 : L키 / LB 버튼
