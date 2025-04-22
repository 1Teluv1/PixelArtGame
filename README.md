# Pixel Brawl (개발 중)

2D 픽셀 아트 액션 전투 게임

## 개요

Pixel Brawl은 Unity로 개발된 2D 픽셀 아트 액션 전투 게임입니다. 플레이어는 다양한 적들과 전투하며 점수를 쌓고 랭킹에 도전할 수 있습니다.

## 개발 환경

- Unity 6 (최신 버전)
- URP (Universal Render Pipeline)
- Visual Scripting (VF)
- WebGL 빌드 타겟

## 주요 기능

- 플레이어 이동, 점프, 공격 시스템
- 콤보 기반 전투 시스템
- 패트롤, 추적, 공격 패턴을 가진 적 AI
- 점수 기반 랭킹 시스템
- 픽셀 아트 그래픽 스타일

## 프로젝트 구조

```
📁 Assets
│
├── Characters    - 플레이어 및 적 캐릭터 리소스
│   ├── Player    - 플레이어 관련 프리팹, 애니메이션 등
│   └── Enemies   - 적 캐릭터 프리팹, 애니메이션 등
│
├── Scripts       - C# 스크립트 파일
│   ├── VFGraphs  - Visual Flow Graph 스크립트
│   ├── Systems   - 게임 시스템 관련 스크립트
│   └── UI        - UI 관련 스크립트
│
├── UI
│   ├── HUD       - 게임 중 HUD 요소
│   └── Menus     - 메뉴 UI 요소
│
├── Art
│   ├── Tilesets  - 레벨 디자인용 타일셋
│   └── FX        - 이펙트 및 파티클 요소
│
└── Scenes        - 게임 씬 파일
    ├── MainScene.unity  - 메인 게임 씬
    └── TestArena.unity  - 테스트용 씬
```

## 주요 스크립트 설명

- `PlayerController.cs`: 플레이어 이동 및 입력 처리
- `EnemyController.cs`: 적 AI 및 행동 패턴 구현
- `CombatSystem.cs`: 전투 메커니즘 및 히트 판정 관리
- `UIManager.cs`: 게임 UI 관리
- `GameManager.cs`: 게임 상태 및 흐름 관리
- `RankingSystem.cs`: 점수 저장 및 랭킹 관리

## 개발 진행 상황

- [x] 기본 프로젝트 구조 설정
- [x] 스크립트 아키텍처 설계
- [ ] 플레이어 이동 및 애니메이션 구현
- [ ] 전투 시스템 구현
- [ ] 적 AI 구현
- [ ] UI 시스템 구현
- [ ] 랭킹 시스템 구현
- [ ] 레벨 디자인
- [ ] 사운드 및 음악 추가
- [ ] 최적화 및 테스트

## 향후 계획

- 멀티 캐릭터 선택 기능
- 다양한 적 유형 및 보스 전투
- 간단한 스토리 모드 or 웨이브 모드
- 점수 기반 랭킹 시스템

## 라이선스

- 이 프로젝트는 개인 학습 목적으로 개발 중입니다.
- 모든 리소스는 별도 표기가 없는 한 원 저작권자에게 있습니다. 