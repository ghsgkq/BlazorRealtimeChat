using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Hubs
{
    [Authorize] // 인증된 사용자만 허브에 연결할 수 있도록 합니다.
    public class ChatHub(IMessageRepository messageRepository) : Hub
    {
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
    }
}
