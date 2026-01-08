using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Hubs
{
    [Authorize] // 인증된 사용자만 허브에 연결할 수 있도록 합니다.
    public class ChatHub : Hub
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
        public async Task SendMessage(string channelId, string message)
        {
            // Context.User.Identity.Name 등을 사용하여 사용자 이름을 가져올 수 있습니다.
            var userName = Context.User.Identity.Name ?? "Unknown User";

            // 메시지를 보낸 클라이언트를 제외한, 같은 그룹(채널)에 있는 모든 다른 클라이언트에게 메시지를 보냅니다.
            await Clients.Group(channelId).SendAsync("ReceiveMessage", userName, message);
        }
    }
}
