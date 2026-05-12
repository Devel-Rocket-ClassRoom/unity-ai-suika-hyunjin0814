# CLAUDE.md — unity-ai-suika-hyunjin0814

## 프로젝트 개요

Aladdin X의 *Suika Game*(2021) 기반 Unity 2D 클론.  
박스 안에 과일을 떨어뜨려 같은 과일끼리 머지시키는 물리 기반 퍼즐 게임.

- **엔진**: Unity 6000.3.15f1 (LTS)
- **렌더 파이프라인**: URP 2D (17.3.0)
- **타겟 플랫폼**: Mobile (iOS / Android) + PC (WebGL / Standalone)
- **GDD**: [Docs/GDD.md](Docs/GDD.md)

---

## 프로젝트 구조 (목표)

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Core/          # GameManager, ScoreManager, AudioManager
    │   ├── Gameplay/      # Dropper, Fruit, FruitManager, GameOverChecker
    │   ├── Data/          # FruitData (ScriptableObject)
    │   └── UI/            # HUDController, PausePanel, GameOverPanel
    ├── Prefabs/           # Fruit prefabs (11종), Box, Dropper
    ├── ScriptableObjects/ # FruitData assets
    ├── Sprites/           # 과일 스프라이트
    ├── Audio/             # BGM + SFX
    └── Scenes/            # Title.unity, Main.unity
```

현재 `Assets/Scenes/SampleScene.unity`만 존재. 코드 작성 시 위 구조로 파일을 배치할 것.

---

## 코어 메커닉 요약

| 요소 | 설명 |
|------|------|
| **Dropper** | 상단에서 좌우 이동 후 낙하. 드롭 후 0.3~0.6초 쿨다운 |
| **Fruit** | `Rigidbody2D` + `CircleCollider2D`. `OnCollisionEnter2D`에서 같은 타입이면 머지 |
| **FruitType** | Cherry(1) → Strawberry(2) → ... → Watermelon(11), 11단계 |
| **GameOverChecker** | 박스 상단 감지선 위로 과일이 일정 시간 체류하면 Game Over |
| **ScoreManager** | 머지마다 (다음 과일 레벨)² × 2 점수 부여 |

---

## 주요 컴포넌트 구현 규칙

- `FruitType` enum 값 = 과일 레벨(1~11). `FruitData` ScriptableObject에 스프라이트·질량·반지름·점수 저장
- 머지는 두 과일 중 먼저 충돌 이벤트를 받은 쪽이 처리 (`isMerging` 플래그로 중복 방지)
- `GameManager`는 싱글턴. 씬 전환 없이 상태(Playing / GameOver)를 관리
- 물리 설정: gravity `-25`, Linear Drag `0.5`, Bounciness `0.3`, Friction `0.1~0.3`
- 입력은 **New Input System** (`InputSystem_Actions.inputactions`) 사용

---

## 코드 스타일

- **포맷터**: CSharpier (`dotnet csharpier <file>`)  
  C# 파일 저장 시 훅으로 자동 실행됨 (`.claude/settings.json` 참고)
- 필드 접근자: `private` 기본, Inspector 노출은 `[SerializeField]`
- 코루틴 이름: `IEnumerator` 반환 메서드에 `Co` 접두사 (`CoMergeCooldown`)
- 이벤트: `System.Action` / `UnityEvent` 혼용 금지 — `System.Action` 통일
- 주석은 **왜(Why)** 가 비자명할 때만 작성

---

## 개발 환경 설정

```powershell
# CSharpier 전역 설치 (최초 1회)
dotnet tool install --global csharpier

# 포맷 수동 실행
dotnet csharpier Assets/_Project/Scripts/
```

Unity 패키지 추가 시 `Packages/manifest.json`을 직접 수정하지 말고 Unity Editor의 Package Manager를 사용할 것.

---

## Unity MCP 도구

Unity Editor가 열려 있는 상태에서 다음 MCP 도구를 사용할 수 있다:

| 도구 | 용도 |
|------|------|
| `Unity_RunCommand` | Editor 메뉴 실행, 빌드 트리거 |
| `Unity_GetConsoleLogs` | 콘솔 로그 확인 |
| `Unity_SceneView_Capture2DScene` | 씬 뷰 스크린샷 |
| `Unity_Camera_Capture` | Game 뷰 캡처 |

---

## 마일스톤

| 단계 | 내용 |
|------|------|
| **M1** | Box + Dropper + 물리 낙하 + 기본 머지 |
| **M2** | 11종 과일 데이터 + 스프라이트 적용 |
| **M3** | 점수 시스템 + HUD + Game Over |
| **M4** | BGM / SFX + 파티클 이펙트 |
| **M5** | 모바일 최적화 + WebGL 빌드 |
