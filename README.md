# 💬 P2P Real-Time Chat (Discord Clone)

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?style=flat-square&logo=blazor&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-RealTime-0078D4?style=flat-square)
![WebRTC](https://img.shields.io/badge/WebRTC-Voice%20Chat-333333?style=flat-square&logo=webrtc&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=flat-square&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker&logoColor=white)

사용자 간의 실시간 텍스트 채팅 및 음성 통화를 지원하는 디스코드(Discord) 스타일의 SPA 웹 애플리케이션입니다. 

**🔗 라이브 데모:** [https://chat.ghsgkq.xyz](https://chat.ghsgkq.xyz)
*(💡 라즈베리파이 홈 서버로 구동 중입니다. 빠른 테스트를 위해 아래 게스트 계정을 사용해 보세요!)*
> **Test Account 1:** `ckdgus5820` / `qwer1234!@`
> **Test Account 2:** `ckdgus1234` / `qwer1234!@`

---

## 📸 Preview (시연 화면)
![image](/img/Preview01.gif)
![image](/img/Preview02.gif)
![image](/img/Preview03.gif)
---

## 🚀 Key Features (핵심 기능)

* **실시간 통신 (SignalR & WebRTC)**
    * **SignalR**을 활용한 지연 없는 텍스트 메시지 및 미디어 파일 전송.
    * **WebRTC**를 이용한 브라우저 간 P2P 음성 통화 및 발화 상태(Speaking Indicator) 실시간 동기화.
* **디스코드형 서버/채널 시스템**
    * 텍스트 채널과 음성 채널의 명확한 분리 및 관리.
* **UX 중심의 파일 업로드**
    * Drag & Drop을 통한 직관적인 파일 업로드 및 실시간 진행률(Progress Bar) 표시.
    * Stream 기반 청크 전송으로 대용량 파일 전송 시 메모리 최적화.
* **유연한 사용자 프로필**
    * `LoginId`(고유계정)와 `UserName`(가변 닉네임)을 DB 단에서 완벽히 분리하여, 자유로운 닉네임/프로필 사진 변경 지원.

---

## 🏗️ Architecture & Technical Highlights (기술적 고민과 해결)

### 1. WebRTC 시그널링 서버의 일원화
WebRTC P2P 통신을 맺기 위한 SDP(Session Description Protocol) 교환 과정을 위해 별도의 시그널링 서버를 구축하는 대신, 기존 채팅에 사용하던 **SignalR Hub를 시그널링 서버로 재활용**했습니다. 이를 통해 백엔드 구조를 단순화하고 소켓 연결 리소스를 최적화했습니다.

### 2. Stream 청크 기반 대용량 파일 처리
이미지/비디오 업로드 시 서버 메모리 부족(OOM) 현상을 방지하기 위해 `ProgressStreamContent`를 직접 구현했습니다. 전체 파일을 한 번에 메모리에 올리지 않고 Stream을 통해 일정 크기(Chunk)로 나누어 전송하며, 이 전송량을 클라이언트 UI의 로딩바에 실시간으로 반영하여 UX를 크게 개선했습니다.

### 3. 라즈베리파이 & Docker 기반 홈 인프라 배포
클라우드 플랫폼에 의존하지 않고, 물리적 라즈베리파이(ARM64) 서버에 직접 인프라를 구축했습니다. `Docker Compose`를 활용해 .NET 서버와 PostgreSQL 컨테이너를 띄우고, 브라우저 마이크 권한 획득(WebRTC 필수 요건)을 위해 `Nginx Proxy Manager`로 역방향 프록시를 구성하고 무료 SSL(Let's Encrypt)을 적용하여 안전한 HTTPS 환경을 완성했습니다.

---

## 🛠️ Tech Stack

* **Frontend:** C# / Blazor WebAssembly / JS Interop / HTML5 / CSS3
* **Backend:** ASP.NET Core 9.0 Web API / SignalR / P2P WebRTC / JWT / BCrypt.Net
* **Database & ORM:** PostgreSQL / Entity Framework Core
* **Infra:** Raspberry Pi / Docker Compose / Nginx Proxy Manager / Let's Encrypt

---

## 💻 Run Locally (로컬 실행 방법)

1. 리포지토리를 클론합니다.
   ```bash
   git clone [https://github.com/your-username/BlazorRealtimeChat.git](https://github.com/your-username/BlazorRealtimeChat.git)

2. 최상위 폴더에서 Docker Compose를 실행하여 DB와 서버를 띄웁니다.
    ```bash
    docker compose up -d --build
    ```

3. 브라우저에서 `http://localhost:8080` 으로 접속합니다.
