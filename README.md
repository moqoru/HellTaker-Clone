# Helltaker (헬테이커) 모작

- 그리드 기반 퍼즐 게임인 [Helltaker](https://store.steampowered.com/app/1289310/Helltaker/)의 Unity, C# 기반 모작입니다.
- 개발 인원 : 1명
- 제작 기간 : 3개월 (2025.10.21 - 2026.01.31)

# 주요 기능

- CSV 기반 레벨 시스템
- 대화 및 선택지 시스템
- 그리드 기반 이동 및 충돌 처리
- 애니메이션 및 이펙트 연출
- 사운드 시스템

# 주요 시스템 상세



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
  - Utils/     - 오브젝트 동작 및 헬퍼 유틸리티
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