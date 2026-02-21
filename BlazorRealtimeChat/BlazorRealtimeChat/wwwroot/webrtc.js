let localStream;
const peerConnections = {};
const iceCandidatesQueue = {}; // PC가 생성되기 전 도착한 후보들을 저장
let dotNetHelper;


// 👇 [수정 1] 전역 변수들을 명확하게 선언해 줍니다.
var audioContext;
let microphone;
let analyser;
let speakingCheckInterval;
let isCurrentlySpeaking = false;

let localScreenStream; // 화면 공유 스트림 저장용

window.webrtcFunctions = {

    // 1. 상대방의 볼륨 조절 (0.0 ~ 1.0)
    setPeerVolume: async (targetId, volume) => {
        const audio = document.getElementById(`audio-${targetId}`);
        if (audio) {
            audio.volume = volume;
        }
    },

    // 2. 상대방 음소거
    setPeerMute: async (targetId, isMuted) => {
        const audio = document.getElementById(`audio-${targetId}`);
        if (audio) {
            audio.muted = isMuted;
        }
    },

    // 3. 내 마이크 켜기/끄기
    setLocalMicEnabled: async (enabled) => {
        if (localStream) {
            localStream.getAudioTracks().forEach(track => {
                track.enabled = enabled;
            });
        }
        return enabled;
    },

    startLocalStream: async (helper) => {
        dotNetHelper = helper;
        try {
            if (!audioContext) {
                audioContext = new (window.AudioContext || window.webkitAudioContext)();
            }
            if (audioContext.state === 'suspended') {
                await audioContext.resume();
                console.log("🔊 AudioContext 미리 기상 완료!");
            }
        } catch (e) {
            console.warn("AudioContext 초기화 에러 (무시 가능):", e);
        }

        try {
            // 이 코드에서 마이크 권한을 물어보며 멈춰있게 됩니다.
            localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
            console.log("✅ 마이크 접근 성공");
            return true;
        } catch (e) {
            console.error("❌ 마이크 접근 실패:", e);
            return false;
        }
    },

    initializeConnection: async (targetId) => {
        console.log(`📡 [Offer 생성] 대상: ${targetId}`);
        const pc = createPeerConnection(targetId);
        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);

        await dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ sdp: pc.localDescription }));
    },

    handleSignal: async (senderId, signalJson) => {
        const signal = JSON.parse(signalJson);
        let pc = peerConnections[senderId];

        if (signal.sdp) {
            console.log(`📥 [SDP 수신] 타입: ${signal.sdp.type}, 발신: ${senderId}`);
            if (!pc) pc = createPeerConnection(senderId);

            await pc.setRemoteDescription(new RTCSessionDescription(signal.sdp));

            if (signal.sdp.type === 'offer') {
                const answer = await pc.createAnswer();
                await pc.setLocalDescription(answer);
                await dotNetHelper.invokeMethodAsync('SendSignalToHex', senderId, JSON.stringify({ sdp: pc.localDescription }));
            }

            // [추가] SDP 처리 후 쌓여있던 ICE 후보들을 모두 적용
            if (iceCandidatesQueue[senderId]) {
                while (iceCandidatesQueue[senderId].length) {
                    const candidate = iceCandidatesQueue[senderId].shift();
                    await pc.addIceCandidate(new RTCIceCandidate(candidate));
                }
            }
        } else if (signal.candidate) {
            console.log(`📥 [ICE Candidate 수신] 발신: ${senderId}`);
            if (pc && pc.remoteDescription) {
                await pc.addIceCandidate(new RTCIceCandidate(signal.candidate));
            } else {
                // PC가 없거나 아직 RemoteDescription이 설정 전이면 큐에 저장
                if (!iceCandidatesQueue[senderId]) iceCandidatesQueue[senderId] = [];
                iceCandidatesQueue[senderId].push(signal.candidate);
            }
        }
    },

    stopStream: () => {
        console.log("Stopping local stream...");
        
        // 1. 마이크 스트림 정지
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }

        // 👇 [추가됨] 2. 화면 공유 스트림도 있으면 정지!
        if (localScreenStream) {
            localScreenStream.getTracks().forEach(track => track.stop());
            localScreenStream = null;
            // C# 쪽에 꺼졌다고 알림
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnScreenShareStopped");
            }
        }

        // 3. UI 초기화
        const localVideo = document.getElementById("local-video");
        if (localVideo) {
             localVideo.srcObject = null;
        }

        // 4. 피어 연결 모두 종료
        for (let id in peerConnections) {
            peerConnections[id].close();
        }
        peerConnections = {};
    },

    // 오디오 분석 시작
    setupAudioAnalysis: async () => {
        if (!localStream) return;

        // 이미 startLocalStream에서 엔진을 깨웠으므로 resume() 코드는 삭제했습니다.
        if (!audioContext) {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }

        // 마이크 스트림을 소스로 연결
        microphone = audioContext.createMediaStreamSource(localStream);
        
        // 분석기 노드 생성
        analyser = audioContext.createAnalyser();
        analyser.fftSize = 512; // 분석 정밀도 설정
        microphone.connect(analyser);

        const dataArray = new Uint8Array(analyser.frequencyBinCount);
        const threshold = 20; // 말하는 것으로 간주할 볼륨 임계값 (조절 가능)

        // 100ms마다 볼륨 체크 루프 시작
        speakingCheckInterval = setInterval(() => {
            analyser.getByteFrequencyData(dataArray);

            // 전체 주파수 대역의 평균 볼륨 계산
            let sum = 0;
            for (let i = 0; i < dataArray.length; i++) {
                sum += dataArray[i];
            }
            const averageVolume = sum / dataArray.length;

            // 임계값과 비교하여 상태 결정
            const isSpeakingNow = averageVolume > threshold;

            // 상태가 변경되었을 때만 C#으로 알림 (불필요한 SignalR 호출 방지)
            if (isSpeakingNow !== isCurrentlySpeaking) {
                isCurrentlySpeaking = isSpeakingNow;
                dotNetHelper.invokeMethodAsync('SetIsSpeakingState', isCurrentlySpeaking);
            }
        }, 100);
    },

    // 오디오 분석 중지
    stopAudioAnalysis: () => {
        if (speakingCheckInterval) clearInterval(speakingCheckInterval);
        if (audioContext) audioContext.close();
        isCurrentlySpeaking = false;
        audioContext = null;
        analyser = null;
        microphone = null;
    },

    // --- 화면 공유 시작 ---
    startScreenShare: async () => {
        try {
            // 1. 브라우저에 화면 공유 권한을 요청합니다.
            localScreenStream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
            const screenTrack = localScreenStream.getVideoTracks()[0];

            // 2. 브라우저 자체의 '공유 중지' 버튼을 눌렀을 때를 대비한 이벤트
            screenTrack.onended = () => {
                window.webrtcFunctions.stopScreenShare();
            };

            // 3. 내 화면을 내 UI에 보여줍니다. (블레이저 쪽에 만들 video 태그 ID)
            const localVideo = document.getElementById("local-video");
            if (localVideo) {
                localVideo.srcObject = localScreenStream;
                localVideo.style.display = "block";
            }

            // 4. 현재 연결된 모든 사람(PeerConnection)에게 내 화면 트랙을 추가합니다.
            for (let id in peerConnections) {
                const pc = peerConnections[id];
                pc.addTrack(screenTrack, localScreenStream);
            }
            return true;
        } catch (e) {
            console.error("화면 공유 취소 또는 에러:", e);
            return false;
        }
    },

    // --- 화면 공유 중지 ---
    stopScreenShare: async () => {
        if (localScreenStream) {
            // 1. 카메라/화면 트랙 끄기
            localScreenStream.getTracks().forEach(track => track.stop());
            localScreenStream = null;

            // 2. 내 UI에서 비디오 숨기기
            const localVideo = document.getElementById("local-video");
            if (localVideo) {
                localVideo.srcObject = null;
                localVideo.style.display = "none";
            }

            // 3. 연결된 피어들에게서 비디오 트랙 제거
            for (let id in peerConnections) {
                const pc = peerConnections[id];
                const senders = pc.getSenders();
                // 비디오 트랙(화면)을 찾아서 제거
                const videoSender = senders.find(s => s.track && s.track.kind === 'video');
                if (videoSender) {
                    pc.removeTrack(videoSender);
                }
            }

            // 4. C# 측에 화면 공유가 꺼졌음을 알림 (버튼 상태 동기화용)
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnScreenShareStopped");
            }
        }
    },
    //특정 비디오 요소를 전체화면으로 띄우거나 끄는 기능 
    toggleFullscreen: (elementId) => {
        const elem = document.getElementById(elementId);
        if (elem) {
            // 현재 전체화면 상태가 아니라면 전체화면 요청
            if (!document.fullscreenElement) {
                if (elem.requestFullscreen) {
                    elem.requestFullscreen().catch(err => console.log(err));
                } else if (elem.webkitRequestFullscreen) { /* 사파리 호환 */
                    elem.webkitRequestFullscreen();
                }
            } else {
                // 이미 전체화면이라면 원래대로 복귀
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                } else if (document.webkitExitFullscreen) { /* 사파리 호환 */
                    document.webkitExitFullscreen();
                }
            }
        }
    }

};

function createPeerConnection(targetId) {
    const pc = new RTCPeerConnection({
        iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
    });

    peerConnections[targetId] = pc;

    if (localStream) {
        localStream.getTracks().forEach(track => pc.addTrack(track, localStream));
    }

    // 👇 [추가] 트랙이 새로 추가되거나 삭제될 때 자동으로 재협상(Offer)을 보냅니다.
    pc.onnegotiationneeded = async () => {
        try {
            // 연결 상태가 안정적일 때만 Offer를 생성해야 충돌이 안 납니다.
            if (pc.signalingState !== "stable") return; 
            
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            await dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ sdp: pc.localDescription }));
        } catch (e) {
            console.error("재협상 에러:", e);
        }
    };

    // 오디오와 비디오(화면)를 구분해서 처리합니다.
    pc.ontrack = (event) => {
        if (event.track.kind === 'audio') {
            let audio = document.getElementById(`audio-${targetId}`);
            if (!audio) {
                audio = document.createElement("audio");
                audio.id = `audio-${targetId}`;
                document.body.appendChild(audio);
            }
            audio.srcObject = event.streams[0];
            audio.play().catch(e => console.error(e));
        } 
        else if (event.track.kind === 'video') {
            
            // 핵심: Blazor에게 "이 사람 공유 켰어! 카드 바꿔줘!" 라고 명령합니다.
            dotNetHelper.invokeMethodAsync('SetUserScreenShareState', targetId, true);

            // Blazor가 카드를 다시 그릴 시간을 0.1초 준 뒤에 비디오 화면을 연결합니다.
            setTimeout(() => {
                let video = document.getElementById(`video-${targetId}`);
                if (video) {
                    video.srcObject = event.streams[0];
                }
            }, 100);

            // 화면 공유가 끝났을 때
            event.track.onended = () => {
                dotNetHelper.invokeMethodAsync('SetUserScreenShareState', targetId, false);
            };
        }
    };

    pc.onicecandidate = (event) => {
        if (event.candidate) {
            dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ candidate: event.candidate }));
        }
    };

    return pc;
}