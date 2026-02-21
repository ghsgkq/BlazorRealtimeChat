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
        console.log(`📡 [연결 초기화] 대상: ${targetId}`);
        // 👇 [핵심 수정] 여기서 강제로 Offer를 만들지 않습니다.
        // createPeerConnection에서 오디오/비디오 트랙을 모두 넣고 나면,
        // 브라우저가 알아서 안전하게 onnegotiationneeded를 한 번만 발생시킵니다!
        createPeerConnection(targetId);
    },

    handleSignal: async (senderId, signalJson) => {
        const signal = JSON.parse(signalJson);
        let pc = peerConnections[senderId];

        if (signal.sdp) {
            console.log(`📥 [SDP 수신] 타입: ${signal.sdp.type}, 발신: ${senderId}`);
            if (!pc) pc = createPeerConnection(senderId);

            try {
                // 👇 [핵심 수정 3] SDP 충돌(Glare) 에러 방어
                // 내가 방금 연결을 요청했는데, 상대방도 동시에 요청해서 꼬인 경우 무시합니다.
                if (signal.sdp.type === 'offer' && pc.signalingState !== 'stable') {
                    console.warn("SDP Offer 충돌 감지. 무시하고 기존 연결을 유지합니다.");
                    return;
                }

                await pc.setRemoteDescription(new RTCSessionDescription(signal.sdp));

                if (signal.sdp.type === 'offer') {
                    const answer = await pc.createAnswer();
                    await pc.setLocalDescription(answer);
                    await dotNetHelper.invokeMethodAsync('SendSignalToHex', senderId, JSON.stringify({ sdp: pc.localDescription }));

                    // 👇 [핵심 추가] 내가 화면을 공유 중이라면 0.5초 뒤에 늦게 온 사람에게 강제로 화면(Offer)을 보냅니다!
                    if (typeof localScreenStream !== 'undefined' && localScreenStream !== null) {
                        setTimeout(async () => {
                            if (pc.signalingState === "stable") {
                                try {
                                    pc.isNegotiating = true;
                                    const offer = await pc.createOffer();
                                    await pc.setLocalDescription(offer);
                                    await dotNetHelper.invokeMethodAsync('SendSignalToHex', senderId, JSON.stringify({ sdp: pc.localDescription }));
                                } catch (e) {
                                    console.error("화면 공유 지연 재협상 에러:", e);
                                } finally {
                                    pc.isNegotiating = false;
                                }
                            }
                        }, 500); // 0.5초 대기
                    }
                }

                if (iceCandidatesQueue[senderId]) {
                    while (iceCandidatesQueue[senderId].length) {
                        const candidate = iceCandidatesQueue[senderId].shift();
                        await pc.addIceCandidate(new RTCIceCandidate(candidate));
                    }
                }
            } catch (e) {
                // 에러가 발생해도 빨간 줄만 띄우고 앱 전체가 터지지 않게 보호합니다.
                console.error("SDP 처리 에러 (앱 보호됨):", e);
            }
        } else if (signal.candidate) {
            if (pc && pc.remoteDescription) {
                pc.addIceCandidate(new RTCIceCandidate(signal.candidate)).catch(e => console.warn(e));
            } else {
                if (!iceCandidatesQueue[senderId]) iceCandidatesQueue[senderId] = [];
                iceCandidatesQueue[senderId].push(signal.candidate);
            }
        }
    },

stopStream: () => {
        console.log("Stopping local stream completely...");
        
        // 마이크 강제 종료
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }

        // 👇 [핵심 방어] 화면 공유 강제 종료 (유령 이벤트 onended 트리거 방지!)
        if (localScreenStream) {
            localScreenStream.getTracks().forEach(track => {
                track.onended = null; // C#을 괴롭히는 찌꺼기 이벤트 삭제
                track.stop();
            });
            localScreenStream = null;
        }

        const localVideo = document.getElementById("local-video");
        if (localVideo) localVideo.srcObject = null;

        // 모든 유령 PeerConnection 파괴
        for (let id in peerConnections) {
            if (peerConnections[id].signalingState !== "closed") {
                peerConnections[id].close();
            }
        }
        peerConnections = {};
        iceCandidatesQueue = {};
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
            if (localScreenStream) {
                localScreenStream.getTracks().forEach(t => t.stop());
            }
            
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
                // 👇 이미 끊어진 연결에는 트랙을 넣지 않도록 방어
                if (pc.signalingState !== "closed") {
                    pc.addTrack(screenTrack, localScreenStream);
                }
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
                // 이미 끊어진 연결에서 트랙을 빼지 않도록 방어
                if (pc.signalingState !== "closed") {
                    const senders = pc.getSenders();
                    const videoSender = senders.find(s => s.track && s.track.kind === 'video');
                    if (videoSender) {
                        pc.removeTrack(videoSender);
                    }
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
    },
    // 나간 사람의 연결을 완전히 부숴버리는 함수
    removeDisconnectedPeers: (activeConnectionIds) => {
        for (let id in peerConnections) {
            if (!activeConnectionIds.includes(id)) {
                console.log(`🔌 [정리] 방을 나간 유저(${id})의 연결을 완전히 종료합니다.`);
                if (peerConnections[id].signalingState !== "closed") {
                    peerConnections[id].close();
                }
                delete peerConnections[id];
                if (iceCandidatesQueue[id]) delete iceCandidatesQueue[id];
            }
        }
    },

};

// webrtc.js 파일 내 위치

function createPeerConnection(targetId) {
    console.log(`🔨 [PC 생성] ID: ${targetId}`); 
    const pc = new RTCPeerConnection({
        iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
    });

    peerConnections[targetId] = pc;

    // 1. 기본 마이크 스트림 연결
    if (localStream) {
        localStream.getTracks().forEach(track => pc.addTrack(track, localStream));
    }

    // 👇 [핵심 수정 1] 내가 이미 화면공유 중일 때, 나중에 들어온 유저에게도 즉시 화면을 보냅니다!
    if (typeof localScreenStream !== 'undefined' && localScreenStream !== null) {
        localScreenStream.getTracks().forEach(track => pc.addTrack(track, localScreenStream));
    }

    pc.isNegotiating = false; // 재협상 락(Lock) 변수

    // 👇 [핵심 수정 2] 재협상(Renegotiation) 시 m-lines 꼬임 방지 안전장치
    pc.onnegotiationneeded = async () => {
        // 이미 협상 중이거나 연결이 불안정하면 무시합니다.
        if (pc.isNegotiating || pc.signalingState !== "stable") return;
        pc.isNegotiating = true;
        try {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            await dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ sdp: pc.localDescription }));
        } catch (e) {
            console.error("재협상 에러 무시 (자연스러운 현상):", e);
        } finally {
            pc.isNegotiating = false;
        }
    };

    pc.ontrack = (event) => {
        if (event.track.kind === 'audio') {
            let audio = document.getElementById(`audio-${targetId}`);
            if (!audio) {
                audio = document.createElement("audio");
                audio.id = `audio-${targetId}`;
                document.body.appendChild(audio);
            }
            audio.srcObject = event.streams[0];
            audio.play().catch(e => console.error("오디오 재생 실패:", e));
        } 
        else if (event.track.kind === 'video') {
            // [핵심 수정] JS에서 강제로 UI를 건드리거나 Blazor를 호출(SetUserScreenShareState)하지 않습니다.
            // UI 상태는 SignalR이 책임지므로, 여기서는 시간차를 두고 영상 데이터만 매핑합니다.
            setTimeout(() => {
                let video = document.getElementById(`video-${targetId}`);
                if (video) {
                    video.srcObject = event.streams[0];
                }
            }, 100);
        }
    };

    pc.onicecandidate = (event) => {
        if (event.candidate) {
            dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ candidate: event.candidate }));
        }
    };

    return pc;
}