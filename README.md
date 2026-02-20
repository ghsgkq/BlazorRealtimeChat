# 💬 Blazor Real-Time Chat (Discord Clone)

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?style=flat-square&logo=blazor&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-RealTime-0078D4?style=flat-square)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=flat-square&logo=postgresql&logoColor=white)
![WebRTC](https://img.shields.io/badge/WebRTC-Voice%20Chat-333333?style=flat-square&logo=webrtc&logoColor=white)

사용자 간의 실시간 텍스트 및 음성 통신을 지원하는 디스코드(Discord) 스타일의 웹 애플리케이션입니다. **Blazor WebAssembly**와 **ASP.NET Core SignalR**을 활용하여 빠르고 반응성 높은 SPA(Single Page Application)를 구축했으며, **WebRTC**를 통해 브라우저 간 P2P 음성 통화를 구현했습니다.

[프로젝트 시연 영상 보러가기](유튜브 링크 또는 GIF 링크) ## 🚀 Key Features (핵심 기능)

* **실시간 채팅 (SignalR)**
    * 새로고침 없이 즉각적으로 메시지를 주고받는 텍스트 채널.
    * 메시지 수신 시 자동 스크롤 및 읽음 처리 최적화.
* **음성 통화 (WebRTC + SignalR Signaling)**
    * 음성 채널 접속 시 사용자 간 P2P 오디오 스트리밍 연결.
    * 개별 사용자 볼륨 조절, 마이크 음소거, 실시간 발화 상태(Speaking Indicator) 시각화.
* **리치 미디어 및 파일 업로드**
    * 드래그 앤 드롭(Drag & Drop)을 통한 직관적인 파일 업로드.
    * 실시간 업로드 진행률 바(Progress Bar) 구현.
    * 이미지 및 비디오 전송 시 썸네일 제공 및 클릭 시 전체 화면 확대 모달 제공.
* **서버 및 채널 기반 아키텍처**
    * 사용자가 직접 서버를 생성하고 소유권(Owner) 획득 및 관리.
    * 서버 내 텍스트/음성 채널 분리 생성.
    * `LoginId` 기반의 서버 초대 시스템 (초대받은 유저만 서버 접근 가능).
* **사용자 인증 및 프로필 관리**
    * JWT(JSON Web Token) 기반의 안전한 인증 및 권한 인가.
    * `LoginId`(불변 계정)와 `UserName`(가변 닉네임)을 분리한 유연한 데이터베이스 설계.
    * 디스코드 스타일의 미니 프로필 모달을 통한 닉네임 및 아바타 이미지 실시간 변경.

## 🛠️ Tech Stack (기술 스택)

**Frontend**
* C# / Blazor WebAssembly
* HTML5 / CSS3 (Discord-like Dark Theme)
* JavaScript Interop (WebRTC API, Drag & Drop API)
* SweetAlert2 (Custom UI Alerts)

**Backend**
* C# / ASP.NET Core Web API
* SignalR (WebSocket 통신 및 WebRTC Signaling Server 역할)
* BCrypt.Net (비밀번호 해싱)

**Database & ORM**
* PostgreSQL
* Entity Framework Core (Code-First Migration)

## 🏗️ Architecture & Technical Highlights (기술적 주안점)

### 1. WebRTC & SignalR 연동 메커니즘
순수 WebRTC는 브라우저 간 직접 연결(P2P)을 지향하지만, 서로를 찾기 위한 **시그널링(Signaling)** 과정이 필수적입니다. 본 프로젝트에서는 별도의 시그널링 서버를 구축하는 대신, 기존 채팅에 사용하던 **SignalR Hub를 시그널링 채널로 재활용**하여 서버 리소스를 최적화하고 구조를 단순화했습니다. `JS Interop`을 통해 Blazor(C#)와 브라우저 API(JS)를 매끄럽게 연결했습니다.

### 2. Stream 처리를 통한 대용량 파일 업로드
대용량 미디어 파일 전송 시 메모리 부족(OOM) 현상을 방지하고 사용자 경험을 향상시키기 위해, 전체 파일을 메모리에 올리지 않고 **스트림(Stream) 기반의 청크 단위 전송**을 구현했습니다. `ProgressStreamContent` 커스텀 클래스를 작성하여 업로드 진행률을 클라이언트 UI(로딩바)에 실시간으로 동기화했습니다.

### 3. 확장성을 고려한 DB 정규화 및 리팩토링
초기에는 `UserName`을 로그인 아이디와 표시 이름으로 혼용했으나, 사용자 편의성(자유로운 닉네임 변경)과 데이터 무결성을 위해 `Id(PK)`, `LoginId(고유 계정)`, `UserName(표시 이름)`으로 역할을 명확히 분리하는 리팩토링을 단행했습니다. 또한, `ServerMember` 조인 테이블을 추가하여 N:M 관계의 다중 사용자-서버 구조를 완성했습니다.

## 💻 Getting Started (실행 방법)

1.  **Repository Clone**
    ```bash
    git clone [https://github.com/your-username/BlazorRealtimeChat.git](https://github.com/your-username/BlazorRealtimeChat.git)
    ```
2.  **Database Setup (PostgreSQL)**
    * `appsettings.json` 파일에서 `DefaultConnection` 문자열을 로컬 DB 정보에 맞게 수정합니다.
    * 마이그레이션 적용 및 DB 생성:
    ```bash
    dotnet ef database update --project BlazorRealtimeChat/BlazorRealtimeChat
    ```
3.  **Run the Application**
    * Visual Studio에서 `BlazorRealtimeChat` (Server 프로젝트)를 시작 프로젝트로 설정하고 실행하거나, 터미널에서 다음 명령어를 입력합니다.
    ```bash
    dotnet run --project BlazorRealtimeChat/BlazorRealtimeChat
    ```

## 📸 Screenshots (스크린샷)

*여기에 기능별 스크린샷을 3~4장 정도 추가하세요.*
* `[스크린샷 1: 전체 채팅 화면 및 다크 테마 UI]`
* `[스크린샷 2: 음성 통화 진행 중 (Speaking Indicator 표출)]`
* `[스크린샷 3: 파일 업로드 진행률 바 및 이미지 확대 모달]`
* `[스크린샷 4: 프로필 수정 모달]`