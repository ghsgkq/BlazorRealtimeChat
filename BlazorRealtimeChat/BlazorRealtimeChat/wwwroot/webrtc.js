let localStream;
const peerConnections = {};
const iceCandidatesQueue = {}; // [추가] PC가 생성되기 전 도착한 후보들을 저장
let dotNetHelper;

window.webrtcFunctions = {
    startLocalStream: async (helper) => {
        dotNetHelper = helper;
        try {
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
        console.log("🛑 WebRTC 모든 연결 및 트랙 종료");
        // 모든 피어 연결 닫기
        Object.keys(peerConnections).forEach(id => {
            if (peerConnections[id]) {
                peerConnections[id].close();
                delete peerConnections[id];
            }
        });

        // 로컬 마이크 트랙 정지
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }
        // ICE 후보 큐 비우기
        Object.keys(iceCandidatesQueue).forEach(id => delete iceCandidatesQueue[id]);
    }
};

function createPeerConnection(targetId) {
    console.log(`🔨 [PC 생성] ID: ${targetId}`); 
    const pc = new RTCPeerConnection({
        iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
    });

    peerConnections[targetId] = pc;

    if (localStream) {
        localStream.getTracks().forEach(track => pc.addTrack(track, localStream));
    }

    pc.ontrack = (event) => {
        console.log(`🔊 [오디오 재생] 상대방(${targetId}) 스트림 수신됨!`);
        let audio = document.getElementById(`audio-${targetId}`);
        if (!audio) {
            audio = document.createElement("audio");
            audio.id = `audio-${targetId}`;
            document.body.appendChild(audio);
        }
        audio.srcObject = event.streams[0];
        audio.autoplay = true;
        audio.play().catch(e => console.error("❌ 오디오 자동 재생 실패:", e));
    };

    pc.onicecandidate = (event) => {
        if (event.candidate) {
            dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ candidate: event.candidate }));
        }
    };

    // 연결 상태 모니터링 로그 추가
    pc.onconnectionstatechange = () => {
        console.log(`🚩 [연결 상태 변경] ${targetId}: ${pc.connectionState}`);
    };

    return pc;
}