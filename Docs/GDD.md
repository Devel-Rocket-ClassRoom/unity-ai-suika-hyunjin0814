# 수박 게임 (Suika Game / Watermelon Game) — Game Design Document

> 본 문서는 Aladdin X사의 *Suika Game*(2021)을 기반으로 한 **Unity 2D 클론 제작용 GDD**입니다.
> 기능 명세 · 컴포넌트 구조 · 구현 우선순위가 포함되어 있습니다.

---

## 1. 게임 개요

**한 줄 요약**
> 박스 안에 떨어뜨린 같은 과일을 충돌시켜 더 큰 과일로 진화시키고, 박스가 넘치기 전에 최대한 높은 점수를 만드는 물리 기반 머지(Merge) 퍼즐 게임.

**장르**: Falling × Merge Puzzle (물리 기반)
**엔진**: Unity 2D (LTS 권장)
**타겟 플랫폼**: Mobile (iOS/Android) + PC (WebGL/Standalone)

### 코어 루프

```
[1] 다음 과일 확인  →  [2] 좌우로 조준해서 드롭  →  [3] 같은 과일끼리 충돌 → 머지 + 점수 획득
        ↑                                                            │
        └──────────────  [4] 박스가 넘치지 않게 공간 관리  ←──────────┘

                        실패 시 → [Game Over] → 재시작
```

---

## 2. 핵심 메커닉 → Unity 2D 구현 매핑

### 2.1 떨어뜨리기 (Drop)

**필요 기능**
- 상단에 "다음 과일"을 들고 좌우로 이동하는 드로퍼(Dropper) 표시
- 입력에 따라 좌우 이동, 클릭/탭/스페이스 시 낙하
- 드롭 직후 *쿨다운*(약 0.3~0.6초) → 연속 드롭으로 인한 박스 폭발 방지

**Unity 구현**

| 항목 | 사용 컴포넌트 / 클래스 |
|:---|:---|
| 드로퍼 이동 | `Dropper.cs` — `Update()`에서 마우스 X 또는 터치 좌표를 박스 폭으로 Clamp |
| 입력 | `Input System` (신) 또는 `Input.GetMouseButtonDown(0)` |
| 드롭 가능 과일 | `FruitType` enum (Cherry~Persimmon, 1~5단계만) 중 가중치 랜덤 |
| 쿨다운 | `bool canDrop` + `Coroutine` |
| Next 프리뷰 | `Dropper`가 현재 + 다음 과일 큐를 들고 UI에 다음 과일 표시 |

```csharp
// 의사 코드
public class Dropper : MonoBehaviour {
    public FruitType current, next;
    private bool canDrop = true;

    void Update() {
        // 좌우 이동: 박스 좌/우 벽 안으로 Clamp
        float x = Mathf.Clamp(GetInputX(), boxLeft + radius, boxRight - radius);
        transform.position = new Vector3(x, dropY, 0);

        if (canDrop && Input.GetMouseButtonDown(0)) {
            SpawnFruit(current, transform.position);
            current = next;
            next = RandomFruit(1, 5); // 1~5단계만
            StartCoroutine(Cooldown());
        }
    }
}
```

---

### 2.2 충돌 (Collision) — 물리 시스템

**필요 기능**
- 모든 과일이 굴러가고 튕기는 자연스러운 2D 물리
- 박스는 좌/우/바닥 3면이 막힌 구조 (상단 개방)
- 과일끼리 끼이거나, 큰 머지의 충격으로 상단 개방부로 과일이 튀어 오르는 예측 불가능성 = 핵심 재미

**Unity 구현**

| 항목 | 컴포넌트 / 설정 |
|:---|:---|
| 과일 콜라이더 | `CircleCollider2D` (반드시 원형 — 굴림성 필수) |
| 강체 | `Rigidbody2D` (Dynamic, Gravity Scale ≈ 1.0~1.5) |
| 박스 벽 | 3개의 `BoxCollider2D` (좌/우/바닥), Static |
| Physics Material 2D | Friction 0.1~0.3, Bounciness 0.05~0.1 (살짝 말랑) |
| 회전 | `freezeRotation = false` (자연스러운 굴림) |
| 충돌 감지 | Collision Detection: `Continuous` (빠른 낙하 시 관통 방지) |
| 수면 방지 | `sleepMode = NeverSleep` 또는 머지 트리거 누락 방지 위해 신중히 |

**Project Settings → Physics 2D**
- Gravity: `(0, -25)` 정도 (게임 느낌에 맞게 튜닝)
- Velocity Iterations: 8, Position Iterations: 3 (기본값 OK)
- Default Contact Offset: 0.01

---

### 2.3 머지 (Merge / Evolution)

**필요 기능**
- 같은 ID의 과일 2개가 충돌 → 두 과일 제거 → 중간점에 다음 단계 과일 생성
- 머지 시 **점수 가산 + 이펙트 + 효과음**
- 한 프레임에 같은 과일끼리 양쪽 충돌이 동시에 일어나도 **중복 머지 방지**

**Unity 구현**

```csharp
public class Fruit : MonoBehaviour {
    public int level;          // 1~11
    public FruitData data;     // ScriptableObject
    private bool merged = false;

    void OnCollisionEnter2D(Collision2D col) {
        if (merged) return;
        var other = col.gameObject.GetComponent<Fruit>();
        if (other == null || other.merged) return;
        if (other.level != level) return;
        if (level >= 11) return; // 수박은 따로 처리

        // 중복 방지: ID 작은 쪽이 머지 주체
        if (GetInstanceID() < other.GetInstanceID()) {
            merged = true;
            other.merged = true;
            FruitManager.Instance.Merge(this, other);
        }
    }
}
```

**FruitManager의 Merge 처리**

1. 두 과일의 중간점 계산
2. 두 GameObject `Destroy`
3. 다음 단계 과일 `Instantiate` (중간점에 생성)
4. 점수 가산 `ScoreManager.Add(newLevel)`
5. 머지 파티클 + 사운드 재생
6. **수박 + 수박 = 양쪽 모두 제거 + 큰 보너스 점수**

**ScriptableObject 구조 (`FruitData`)**

| 필드 | 타입 | 설명 |
|:---|:---|:---|
| level | int | 1~11 |
| displayName | string | "체리", "딸기"... |
| sprite | Sprite | 과일 이미지 |
| radius | float | 콜라이더 반지름 |
| score | int | 머지 시 획득 점수 |
| mergeSfx | AudioClip | 머지 효과음 (단계별 음정 ↑) |
| mass | float | 단계가 클수록 무겁게 |

---

### 2.4 게임오버 (Game Over)

**필요 기능**
- 박스 상단의 **레드라인**을 일정 시간(약 1.5~2초) 이상 침범하면 종료
- 단, **머지 직후의 일시적 튀어오름은 유예** → 즉시 게임오버 X
- 게임오버 시 BGM 정지, 결과 패널, 베스트 스코어 갱신

**Unity 구현**

```csharp
public class GameOverChecker : MonoBehaviour {
    public float lineY;          // 레드라인 Y좌표
    public float graceTime = 2f;
    private Dictionary<Fruit, float> overTimers = new();

    void Update() {
        foreach (var fruit in FruitManager.Instance.AllFruits) {
            if (fruit.transform.position.y > lineY && fruit.IsResting()) {
                overTimers[fruit] = overTimers.GetValueOrDefault(fruit, 0) + Time.deltaTime;
                if (overTimers[fruit] >= graceTime) {
                    GameManager.Instance.GameOver();
                }
            } else {
                overTimers.Remove(fruit);
            }
        }
    }
}

// Fruit 측: "안정 상태"인지 판단
public bool IsResting() => rb.linearVelocity.magnitude < 0.1f;
```

> ⚠ 머지로 튀어오른 직후에는 속도가 크므로 자연스럽게 카운트되지 않음.

---

## 3. 과일 단계 (11종)

| 단계 | 과일 | 한국어명 | 상대 반지름 | 머지 시 점수 | 드롭 가능 |
|:---:|:---:|:---|:---:|:---:|:---:|
| 1 | 🍒 | 체리 | 0.30 | — | ✅ |
| 2 | 🍓 | 딸기 | 0.40 | +3 | ✅ |
| 3 | 🍇 | 포도 | 0.50 | +6 | ✅ |
| 4 | 🍊 | 데코폰 | 0.65 | +10 | ✅ |
| 5 | 🍊 | 감 | 0.80 | +15 | ✅ |
| 6 | 🍎 | 사과 | 0.95 | +21 | ❌ |
| 7 | 🍐 | 배 | 1.15 | +28 | ❌ |
| 8 | 🍑 | 복숭아 | 1.35 | +36 | ❌ |
| 9 | 🍍 | 파인애플 | 1.60 | +45 | ❌ |
| 10 | 🍈 | 멜론 | 1.85 | +55 | ❌ |
| 11 | 🍉 | 수박 | 2.15 | +66 | ❌ |

> 점수 공식: **N(N+1)/2** (N = 머지로 생성된 과일의 단계). 튜닝 가능하도록 ScriptableObject 필드로 분리.
> **수박 2개 머지** = 양쪽 제거 + 보너스 +100 (또는 별도 룰).

---

## 4. 점수 · 승패 규칙

### 점수
- 머지 시점에 새로 생성된 과일의 `score` 값 가산
- 누적 점수는 `ScoreManager` (Singleton)에서 관리, `PlayerPrefs`로 베스트 스코어 영구 저장
- 화면 상단 UI에 실시간 표시, 머지 시 `+점수 팝업` (DOTween으로 fade-up)

### 승리/패배
- 명시적 승리 없음 → **하이스코어 갱신이 목표**
- 패배 조건: 레드라인 위 체류 2초 이상

---

## 5. 화면 · UI 구성 (Unity Canvas)

### 5.1 레이아웃 (세로 모드 기준, 1080x1920)

```
┌─────────────────────────────────┐
│ SCORE: 2,475          ⚙️ ⏸️    │  ← 상단 HUD (Canvas: Screen Space - Overlay)
├─────────────────────────────────┤
│      🤖(Poppy + 🍒)             │  ← Dropper (World Space, Box 상단)
│   - - - - - - - - - - - - - -   │  ← Red Line (LineRenderer 또는 Sprite)
│ ┌─────────────────────────────┐ │
│ │                             │ │
│ │           🍉                │ │
│ │       🍈        🍑          │ │  ← Play Box (3 BoxCollider2D + Sprite 벽)
│ │     🍎  🍐  🍇  🍓          │ │
│ │   🍒  🍊  🍒  🍓  🍇        │ │
│ │ 🍒 🍓 🍇 🍒 🍓 🍒 🍇 🍊    │ │
│ └─────────────────────────────┘ │
│                                 │
│  NEXT: 🍓        BEST: 5,120    │  ← 하단 UI
└─────────────────────────────────┘
```

### 5.2 UI 컴포넌트 매핑

| UI 요소 | Unity 구현 |
|:---|:---|
| Score 텍스트 | `TextMeshProUGUI`, `ScoreManager.OnScoreChanged` 이벤트로 갱신 |
| Next 프리뷰 | `Image` + `Dropper.OnNextChanged` 이벤트 |
| Best 텍스트 | `PlayerPrefs.GetInt("BestScore")` |
| 일시정지 패널 | `GameObject.SetActive` + `Time.timeScale = 0` |
| 게임오버 패널 | 결과 점수 표시 + Retry / Quit 버튼 |
| 진화 차트 | 11개 과일 스프라이트 횡렬 (선택적, 토글 UI) |

---

## 6. Unity 프로젝트 구조 (권장)

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs        // 게임 상태 머신
│   │   │   ├── ScoreManager.cs       // 점수 + 베스트
│   │   │   └── AudioManager.cs       // BGM + SFX
│   │   ├── Gameplay/
│   │   │   ├── Dropper.cs            // 드로퍼 (입력 + 스폰)
│   │   │   ├── Fruit.cs              // 개별 과일 + 충돌 감지
│   │   │   ├── FruitManager.cs       // 머지 처리 + 풀링
│   │   │   ├── FruitFactory.cs       // 프리팹 인스턴스화
│   │   │   └── GameOverChecker.cs    // 레드라인 체크
│   │   ├── Data/
│   │   │   └── FruitData.cs          // ScriptableObject
│   │   └── UI/
│   │       ├── HUD.cs                // Score/Next/Best UI
│   │       ├── PausePanel.cs
│   │       └── GameOverPanel.cs
│   ├── Prefabs/
│   │   ├── Fruits/                   // 11개 과일 프리팹
│   │   ├── Box.prefab
│   │   └── Dropper.prefab
│   ├── ScriptableObjects/
│   │   └── FruitData/                // 11개 .asset
│   ├── Sprites/
│   ├── Audio/
│   │   ├── BGM/
│   │   └── SFX/
│   └── Scenes/
│       ├── Main.unity
│       └── Title.unity
```

---

## 7. 구현 우선순위 (마일스톤)

### M1 — 프로토타입 (코어 메커닉만)
- [ ] 2D 박스 + 중력
- [ ] 1종 과일 드롭 → 같은 과일 2개 충돌 시 1단계 위로 머지
- [ ] 점수 텍스트 (콘솔 출력 수준 OK)

### M2 — 전체 게임 사이클
- [ ] 과일 11종 등록 (ScriptableObject 11개)
- [ ] 드로퍼 좌우 이동 + 쿨다운
- [ ] 1~5단계만 드롭, 그 외는 머지로만 생성
- [ ] 레드라인 + 게임오버 판정
- [ ] Score UI + Best Score (PlayerPrefs)

### M3 — 폴리시 (재미의 80%는 여기서)
- [ ] 머지 파티클 + 효과음 (단계별 음정 ↑)
- [ ] +점수 팝업 애니메이션 (DOTween)
- [ ] BGM 루프 + 게임오버 시 페이드아웃
- [ ] 카메라 살짝 흔들림 (큰 머지 시)
- [ ] 박스 벽 살짝 떨림 (큰 과일 낙하 시)

### M4 — 확장
- [ ] 일시정지 / 재시작
- [ ] 수박 + 수박 = 보너스 룰
- [ ] 진화 차트 UI
- [ ] 세팅 (BGM/SFX 볼륨)
- [ ] 모바일 터치 최적화 + 가로/세로 대응

### M5 — 선택 사항
- [ ] 시즌 스킨 (할로윈, 크리스마스)
- [ ] 리더보드 (Firebase / Unity Cloud)
- [ ] 광고 / IAP

---

## 8. 톤 & 분위기

### 8.1 비주얼
- **귀엽고 친근함** — Aladdin X 측이 인기 비결로 직접 언급한 키워드
- 모든 과일에 **간단한 눈 + 입 표정**
- **파스텔톤 + 따뜻한 베이지 배경**, 만화풍 라운드 실루엣
- 그림자는 부드럽게, 무게감보다 *말랑한 느낌*

### 8.2 사운드
- **메인 BGM**: 리코더 + 멜로디카 + 가벼운 퍼커션, 동요풍 무한 루프
- **머지 SFX**: 단계가 올라갈수록 음정이 높아지는 "퐁" 사운드 — **이것이 만족감의 80%**
- **드롭 SFX**: 가벼운 "통"
- **게임오버**: 짧은 슬픈 멜로디 — 좌절감보다 *귀여운 실패감*

### 8.3 톤 한 줄
> *"누가 봐도 만지고 싶고, 5분만 하려다 한 시간이 지나는, 햇살 좋은 오후의 게임."*

---

## 9. 기술적 주의사항 ⚠

| 이슈 | 해결 방법 |
|:---|:---|
| **머지 중복 발생** | `bool merged` 플래그 + `InstanceID` 비교로 한쪽만 트리거 |
| **고속 낙하 시 박스 관통** | `Rigidbody2D.collisionDetectionMode = Continuous` |
| **연쇄 머지로 인한 폭발** | 머지 결과물에 약간의 위쪽 추력 X (자연스러운 물리로만) |
| **수면 상태로 머지 누락** | `sleepMode = NeverSleep` 또는 머지 직후 `WakeUp()` 호출 |
| **모바일 성능** | 객체 풀링 (`ObjectPool<Fruit>`), 파티클은 짧고 가볍게 |
| **레드라인 오작동** | "안정 상태에서만 카운트" 로직 필수 (`velocity.magnitude < 0.1f`) |
| **빌드 사이즈** | 11개 스프라이트는 한 장의 Sprite Atlas로 묶기 |

---

## 10. 부록 — 추천 에셋 / 라이브러리

| 용도 | 추천 |
|:---|:---|
| 트윈 애니메이션 | **DOTween** (Free) |
| 객체 풀링 | Unity 내장 `ObjectPool<T>` (2021 LTS+) |
| 사운드 관리 | 간단하면 자체 `AudioManager`, 복잡하면 **FMOD** |
| UI | **TextMeshPro** (필수) |
| 입력 | **Input System** (신, 모바일 터치 대응 쉬움) |

---

*문서 버전 v2.0 (Unity 2D 구현 명세 포함) · 작성일 2026-05-12*