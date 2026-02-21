using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Hubs
{
    [Authorize] // 인증된 사용자만 허브에 연결할 수 있도록 합니다.
    public class ChatHub(IMessageRepository messageRepository) : Hub
    {
        // {채널ID, 유저리스트} 형태의 정적 저장소
        private static readonly ConcurrentDictionary<string, List<VoiceUserDto>> VoiceUsers = new();

        // 접속한 유저를 추적하기 위한 전역 메모리 
        public static readonly ConcurrentDictionary<string, string> OnlineUsers = new();


          // --- 클라이언트 접속 시 온라인 처리 ---
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // 유저가 접속하면 딕셔너리에 추가하고, 모두에게 "이 유저 온라인!" 이라고 알립니다.
                OnlineUsers.TryAdd(Context.ConnectionId, userId);
                await Clients.All.SendAsync("UserOnline", userId);
            }
            
            await base.OnConnectedAsync();
        }

        // 연결이 끊어지면 실행 (새로고침, 브라우저 닫기 대응)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // 1. 기존 WebRTC 정리 로직
            await CleanupUserFromAllGroups();

            // 2. 온라인 상태 관리 로직
            if (OnlineUsers.TryRemove(Context.ConnectionId, out var userId))
            {
                // 같은 유저가 다른 탭(기기)으로 접속 중인지 확인합니다.
                bool isStillOnline = OnlineUsers.Values.Contains(userId);
                
                // 완전히 접속이 끊겼다면 모두에게 "이 유저 오프라인!" 이라고 알립니다.
                if (!isStillOnline)
                {
                    await Clients.All.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        

        // 클라이언트가 채널에 참여하기 위해 호출하는 메소드
        public async Task JoinChannel(string channelId)
        {
            // SignalR의 "그룹" 기능을 사용하여, 각 채널을 별도의 채팅방으로 만듭니다.
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }

        // 클라이언트가 채널에서 나가기 위해 호출하는 메소드
        public async Task LeaveChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        // 클라이언트가 메시지를 보내기 위해 호출하는 메소드
        public async Task SendMessage(string channelId, string userProfileUrl, string message)
        {
   

            // Context.User.Identity.Name 등을 사용하여 사용자 이름을 가져올 수 있습니다.
            var userName = Context.User.Identity.Name ?? "Unknown User";
            var userId = Context.UserIdentifier;

            // 메시지 엔티티 생성 및 DB 저장
            var newMessage = new Message
            {
                Content = message,
                FileUrl = null,
                ChannelId = Guid.Parse(channelId),
                UserId = Guid.Parse(userId ?? Guid.Empty.ToString()),
                Timestamp = DateTime.UtcNow
            };

            // 메시지 추가
            await messageRepository.AddMessageAsync(newMessage);

            // 메시지를 보낸 클라이언트를 제외한, 같은 그룹(채널)에 있는 모든 다른 클라이언트에게 메시지를 보냅니다.
            await Clients.Group(channelId).SendAsync("ReceiveMessage", userName, userProfileUrl, message);
        }

        // 파일 메시지를 보내기 위해 호출하는 메소드
        public async Task SendMessageWithFile(string channelId,string userProfileUrl, string fileUrl, string messageType)
        {
            try
            {
                var userName = Context.User.Identity.Name ?? "Unknown User";

                // 현재 TokenService는 'sub'에 Username을 넣고 있습니다.
                var userId = Context.UserIdentifier;

                Console.WriteLine($"[Debug] User: {userName}, ID: {userId}, Channel: {channelId}");

                var newMessage = new Message
                {
                    Content = messageType == "image" ? "이미지를 보냈습니다." : "동영상을 보냈습니다.",
                    FileUrl = fileUrl,
                    MessageType = messageType,
                    ChannelId = Guid.Parse(channelId),
                    UserId = Guid.Parse(userId ?? Guid.Empty.ToString()),
                    Timestamp = DateTime.UtcNow
                };

                await messageRepository.AddMessageAsync(newMessage);

                await Clients.Group(channelId).SendAsync("ReceiveMessageWithFile", userName, userProfileUrl, fileUrl, messageType);
            }
            catch (Exception ex)
            {
                // 서버 콘솔(검은 창)에 에러 내용이 자세히 출력됩니다.
                Console.WriteLine($"[ChatHub Error] {ex.Message}");
                Console.WriteLine($"[Stack Trace] {ex.StackTrace}");
                throw; // 에러를 다시 던져서 필요시 서버 로그 시스템에 기록되게 합니다.
            }
        }

        //-- WebRTC 관련 메서드 --//
        
        // 특정 대상에게 WebRTC 신호를 보내는 메서드
    
        public async Task SendSignal(string targetConnectionId, string signal)
        {
            // 보낸 사람의 ID를 포함하여 대상에게 전달
            await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signal);
        }

        // 음성 채널 그룹에 입장하고 다른 사람들에게 알립니다.
        public async Task JoinVoiceGroup(string channelId, string userName, string profileUrl)
        {
            // 입장 전 다른 방(혹은 같은 방의 이전 세션)에 남아있던 내 정보 제거 (중복 방지)
            await CleanupUserFromAllGroups();

            string groupName = $"voice-{channelId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var user = new VoiceUserDto
            {
                ConnectionId = Context.ConnectionId,
                UserName = userName,
                ProfileImageUrl = profileUrl
            };

            var users = VoiceUsers.GetOrAdd(channelId, _ => new List<VoiceUserDto>());
            lock (users) { users.Add(user); }

            // 방 전체에 업데이트 알림
            await BroadcastVoiceUpdate(channelId);
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedVoice", Context.ConnectionId);
        }

        // 음성 채널 퇴장 알림
        public async Task LeaveVoiceGroup(string channelId)
        {
            await RemoveUserFromGroup(channelId);
        }

        // 모든 채널에서 현재 연결(ConnectionId)을 찾아 제거하는 헬퍼 메서드
        private async Task LeaveAllVoiceGroups()
        {
            foreach (var channelId in VoiceUsers.Keys)
            {
                if (VoiceUsers.TryGetValue(channelId, out var users))
                {
                    bool removed;
                    lock (users) { removed = users.RemoveAll(u => u.ConnectionId == Context.ConnectionId) > 0; }

                    if (removed)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"voice-{channelId}");
                        await BroadcastVoiceUpdate(channelId);
                    }
                }
            }
        }

        private async Task RemoveUserFromGroup(string channelId)
        {
            string groupName = $"voice-{channelId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            if (VoiceUsers.TryGetValue(channelId, out var users))
            {
                lock (users) { users.RemoveAll(u => u.ConnectionId == Context.ConnectionId); }
                await BroadcastVoiceUpdate(channelId);
            }
        }

        private async Task CleanupUserFromAllGroups()
        {
            foreach (var channelId in VoiceUsers.Keys)
            {
                if (VoiceUsers.TryGetValue(channelId, out var users))
                {
                    bool removed;
                    lock (users) { removed = users.RemoveAll(u => u.ConnectionId == Context.ConnectionId) > 0; }
                    if (removed)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"voice-{channelId}");
                        await BroadcastVoiceUpdate(channelId);
                    }
                }
            }
        }

        private async Task BroadcastVoiceUpdate(string channelId)
        {
            if (VoiceUsers.TryGetValue(channelId, out var users))
            {
                // 해당 채널 그룹에 있는 모든 사람에게 최신 명단 전송
                await Clients.Group($"voice-{channelId}").SendAsync("UpdateVoiceUsers", channelId, users);

                // 만약 서버를 옮겼거나 하는 경우를 대비해 전체 사용자에게 브로드캐스트 (디코 스타일 사이드바 동기화)
                await Clients.All.SendAsync("UpdateVoiceUsers", channelId, users);
            }
        }

        public async Task<IDictionary<string, List<VoiceUserDto>>> GetVoiceUsersState()
        {
            // 현재 메모리에 저장된 모든 음성 채널의 유저 정보를 반환합니다.
            return VoiceUsers;
        }

        // 말하는 상태 변경 알림 (채널 ID는 클라이언트가 현재 접속 중인 채널을 보내줌)
        public async Task SendSpeakingState(string channelId, bool isSpeaking)
        {
            // 나를 포함한 그룹 전체에게 알림 (내 UI도 업데이트되어야 하므로)
            await Clients.Group($"voice-{channelId}").SendAsync("ReceiveSpeakingState", Context.ConnectionId, isSpeaking);
        }


        //-- WebRTC 관련 메서드 --//


    }
}
