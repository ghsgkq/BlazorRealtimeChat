let localStream;
let localScreenStream;
let peerConnections = {};
let dotNetHelper;
let iceCandidatesQueue = {};
let audioContext;
let analyzer;
let microphone;
let javascriptNode;

window.webrtcFunctions = {
    startLocalStream: async (dotNetRef) => {
        try {
            // 💡 [핵심] 기존에 켜져있던 마이크가 있다면 확실하게 죽이고 시작합니다.
            if (localStream) {
                localStream.getTracks().forEach(t => t.stop());
            }
            localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
            dotNetHelper = dotNetRef;
            return true;
        } catch (e) {
            console.error("마이크 권한 거부 또는 에러:", e);
            return false;
        }
    },

    setupAudioAnalysis: () => {
        try {
            if (!localStream) return;
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
            analyzer = audioContext.createAnalyser();
            microphone = audioContext.createMediaStreamSource(localStream);
            javascriptNode = audioContext.createScriptProcessor(2048, 1, 1);

            analyzer.smoothingTimeConstant = 0.8;
            analyzer.fftSize = 1024;

            microphone.connect(analyzer);
            analyzer.connect(javascriptNode);
            javascriptNode.connect(audioContext.destination);

            let isSpeaking = false;
            javascriptNode.onaudioprocess = () => {
                var array = new Uint8Array(analyzer.frequencyBinCount);
                analyzer.getByteFrequencyData(array);
                var values = 0;
                var length = array.length;
                for (var i = 0; i < length; i++) {
                    values += (array[i]);
                }
                var average = values / length;

                let speakingNow = average > 15;
                if (isSpeaking !== speakingNow) {
                    isSpeaking = speakingNow;
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync('SetIsSpeakingState', isSpeaking);
                    }
                }
            }
        } catch (e) {
            console.error("오디오 분석기 세팅 실패:", e);
        }
    },

    stopAudioAnalysis: () => {
        if (javascriptNode) {
            javascriptNode.disconnect();
            javascriptNode.onaudioprocess = null;
            javascriptNode = null;
        }
        if (analyzer) analyzer.disconnect();
        if (microphone) microphone.disconnect();
        if (audioContext) {
            audioContext.close();
            audioContext = null;
        }
    },

    startScreenShare: async () => {
        try {
            if (localScreenStream) {
                localScreenStream.getTracks().forEach(t => t.stop());
            }

            localScreenStream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
            const screenTrack = localScreenStream.getVideoTracks()[0];

            screenTrack.onended = () => {
                window.webrtcFunctions.stopScreenShare();
            };

            const localVideo = document.getElementById("local-video");
            if (localVideo) {
                localVideo.srcObject = localScreenStream;
            }

            for (let id in peerConnections) {
                const pc = peerConnections[id];
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

    stopScreenShare: async () => {
        if (localScreenStream) {
            localScreenStream.getTracks().forEach(track => {
                track.onended = null;
                track.stop();
            });
            localScreenStream = null;

            const localVideo = document.getElementById("local-video");
            if (localVideo) localVideo.srcObject = null;

            for (let id in peerConnections) {
                const pc = peerConnections[id];
                if (pc.signalingState !== "closed") {
                    const senders = pc.getSenders();
                    const videoSender = senders.find(s => s.track && s.track.kind === 'video');
                    if (videoSender) {
                        pc.removeTrack(videoSender);
                    }
                }
            }

            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync("OnScreenShareStopped");
            }
        }
    },

    stopStream: () => {
        console.log("🔌 마이크, 화면공유, 통신 선 강제 파괴 중...");
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }

        if (localScreenStream) {
            localScreenStream.getTracks().forEach(track => {
                track.onended = null;
                track.stop();
            });
            localScreenStream = null;
        }

        const localVideo = document.getElementById("local-video");
        if (localVideo) localVideo.srcObject = null;

        for (let id in peerConnections) {
            if (peerConnections[id].signalingState !== "closed") {
                peerConnections[id].close();
            }
        }
        peerConnections = {};
        iceCandidatesQueue = {};

        // 💡 [핵심 해결] 브라우저에 남아있던 상대방의 '좀비 오디오 태그'를 싹 다 불태웁니다! (먹통 방지)
        document.querySelectorAll('audio[id^="audio-"]').forEach(el => el.remove());
    },

    removeDisconnectedPeers: (activeConnectionIds) => {
        for (let id in peerConnections) {
            if (!activeConnectionIds.includes(id)) {
                if (peerConnections[id].signalingState !== "closed") {
                    peerConnections[id].close();
                }
                delete peerConnections[id];
                if (iceCandidatesQueue[id]) delete iceCandidatesQueue[id];
                
                // 나간 사람의 좀비 오디오 태그도 즉시 제거
                let oldAudio = document.getElementById(`audio-${id}`);
                if (oldAudio) oldAudio.remove();
            }
        }
    },

    initializeConnection: async (targetId) => {
        createPeerConnection(targetId);
    },

    handleSignal: async (senderId, signalJson) => {
        const signal = JSON.parse(signalJson);
        let pc = peerConnections[senderId];

        if (signal.sdp) {
            if (!pc) pc = createPeerConnection(senderId);

            // 💡 [핵심 해결] 처리 중에 Offer/Answer가 꼬이지 않도록 자물쇠(Lock)를 채웁니다!
            pc.isHandlingSignal = true;

            try {
                if (signal.sdp.type === 'offer' && pc.signalingState !== 'stable') return;

                await pc.setRemoteDescription(new RTCSessionDescription(signal.sdp));

                if (signal.sdp.type === 'offer') {
                    const answer = await pc.createAnswer();
                    await pc.setLocalDescription(answer);
                    await dotNetHelper.invokeMethodAsync('SendSignalToHex', senderId, JSON.stringify({ sdp: pc.localDescription }));

                    if (typeof localScreenStream !== 'undefined' && localScreenStream !== null) {
                        setTimeout(async () => {
                            if (pc.signalingState === "stable") {
                                pc.isNegotiating = true;
                                try {
                                    const offer = await pc.createOffer();
                                    await pc.setLocalDescription(offer);
                                    await dotNetHelper.invokeMethodAsync('SendSignalToHex', senderId, JSON.stringify({ sdp: pc.localDescription }));
                                } catch(e){} finally {
                                    pc.isNegotiating = false;
                                }
                            }
                        }, 500);
                    }
                }
            } catch (e) {
                console.error("SDP 처리 에러:", e);
            } finally {
                if (pc) pc.isHandlingSignal = false; // 작업 끝나면 자물쇠 해제
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

    setPeerVolume: (connectionId, volume) => {
        let audio = document.getElementById(`audio-${connectionId}`);
        if (audio) audio.volume = volume;
    },

    setPeerMute: (connectionId, isMuted) => {
        let audio = document.getElementById(`audio-${connectionId}`);
        if (audio) audio.muted = isMuted;
    },

    setLocalMicEnabled: (enabled) => {
        if (localStream) {
            localStream.getAudioTracks().forEach(track => {
                track.enabled = enabled;
            });
        }
    },

    toggleFullscreen: (elementId) => {
        const elem = document.getElementById(elementId);
        if (elem) {
            if (!document.fullscreenElement) {
                if (elem.requestFullscreen) elem.requestFullscreen().catch(e => console.log(e));
                else if (elem.webkitRequestFullscreen) elem.webkitRequestFullscreen();
            } else {
                if (document.exitFullscreen) document.exitFullscreen();
                else if (document.webkitExitFullscreen) document.webkitExitFullscreen();
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

    if (typeof localScreenStream !== 'undefined' && localScreenStream !== null) {
        localScreenStream.getTracks().forEach(track => pc.addTrack(track, localScreenStream));
    }

    pc.isNegotiating = false;
    pc.isHandlingSignal = false;

    // 💡 Lock 방어 코드로 꼬임 완벽 방지
    pc.onnegotiationneeded = async () => {
        if (pc.isNegotiating || pc.isHandlingSignal || pc.signalingState !== "stable") return;
        pc.isNegotiating = true;
        try {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            await dotNetHelper.invokeMethodAsync('SendSignalToHex', targetId, JSON.stringify({ sdp: pc.localDescription }));
        } catch (e) {
            console.error("재협상 무시:", e);
        } finally {
            pc.isNegotiating = false;
        }
    };

    pc.ontrack = (event) => {
        if (event.track.kind === 'audio') {
            // 💡 [핵심 해결] 기존 오디오 태그를 찢고 완전히 새로 만듦 (크롬 버그 방지)
            let oldAudio = document.getElementById(`audio-${targetId}`);
            if (oldAudio) oldAudio.remove();

            let audio = document.createElement("audio");
            audio.id = `audio-${targetId}`;
            audio.autoplay = true;
            document.body.appendChild(audio);
            audio.srcObject = event.streams[0];
            audio.play().catch(e => console.log("자동재생 대기중"));
        } 
        else if (event.track.kind === 'video') {
            dotNetHelper.invokeMethodAsync('SetUserScreenShareState', targetId, true);
            
            // 💡 0.2초 대기하여 Blazor가 Video 태그를 DOM에 렌더링할 시간을 충분히 줍니다.
            setTimeout(() => {
                let video = document.getElementById(`video-${targetId}`);
                if (video) video.srcObject = event.streams[0];
            }, 200);

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