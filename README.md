# 오목 복싱

**지는 순간 판을 엎고 주먹으로 승부를 보는 오목 게임**

![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white)
![C Sharp](https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=csharp&logoColor=white)
![Cinemachine](https://img.shields.io/badge/Cinemachine-334155?style=flat-square)

<!-- TODO: 오목에서 복싱으로 전환되는 장면 GIF. 이 게임의 핵심이라 영상이 가장 잘 설명합니다 -->

## 프로젝트 소개

AI와 오목을 둡니다. 그러다 판세가 기울면, 바둑판을 엎고 한 판 붙습니다.

보드게임으로 시작해 액션으로 넘어가는 전환 자체를 재미 요소로 잡은 프로젝트입니다. 오목판 앞에 앉아 있던 캐릭터가 자리에서 일어나면 카메라가 3인칭 전투 시점으로 바뀌고, 그때부터는 체력을 걸고 싸웁니다.

## 주요 시스템

| 영역 | 구현 내용 |
|---|---|
| 오목 | 보드 판정, 승패 처리, 착수 규칙 |
| 오목 AI | Zobrist Hashing 기반 전치 테이블로 탐색 결과를 캐싱한 착수 선택 |
| 전투 | 플레이어·보스 공격 판정, 히트박스, 피격과 체력 |
| 전환 | 오목 시점에서 전투 시점으로의 카메라 전환 |
| 커스터마이징 | 의상 카탈로그와 슬롯 기반 착용 |
| 그 외 | 인트로, 팝업, 사운드, 튜토리얼 UI |

## 기술 스택

Unity · C# · Cinemachine · Input System · Photon

## 개인 기여 — 최서영

- 캐릭터 의상 커스터마이징 시스템 — 카탈로그, 슬롯 타입, UI 버튼 바인딩
- 전투 히트박스와 피격 판정 (플레이어, 보스)
- 체력 바와 전투 UI
- 오목에서 복싱으로 넘어갈 때의 카메라 전환 제어
- 씬 부트스트랩과 캐릭터 트랜스폼 관리

<!-- TODO: 카메라 전환을 자연스럽게 만들며 겪은 문제가 있으면 2~3줄 -->

## 프로젝트 구조

```text
Assets/
├── 01. Scenes/
│   ├── Rooms/           # 오목·전투 진행 씬
│   ├── MergeIntro       # 인트로
│   └── PopUp
└── 02. Scripts/
    ├── Omok Single/     # 보드 로직과 AI
    ├── AIFight/         # 보스 AI와 히트박스
    ├── BattleScripts/   # 전투 컨트롤러와 공격 판정
    ├── Custom/          # 의상 커스터마이징
    ├── Fight_UI/        # 체력 바, 전투 UI
    ├── Common/          # 씬 부트스트랩, 카메라
    ├── Intro/  PopUp/  Sound/  Managers/
    └── ...
```
