using Microsoft.AspNetCore.SignalR;

namespace BlazorRealtimeChat.Hubs;

public class ChatHub : Hub
{
    // 클라이언트가 이 메서드를 호출하여 특정 채널로 메시지를 보냅니다.
    public async Task SendMessageToChannel(string channelId, string user, string message)
    {
        // 메시지를 보낸 채널(그룹)에 속한 모든 클라이언트에게 메시지를 전달합니다.
        await Clients.Group(channelId).SendAsync("ReceiveMessage", user, message);
    }

    // 클라이언트가 특정 채널에 참여할 때 호출합니다.
    public async Task JoinChannel(string channelId)
    {
        // 현재 연결을 특정 채널 ID를 이름으로 하는 그룹에 추가합니다.
        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
    }

    // 클라이언트가 다른 채널로 이동하거나 연결을 끊을 때 호출합니다.
    public async Task LeaveChannel(string channelId)
    {
        // 현재 연결을 특정 채널 ID를 이름으로 하는 그룹에서 제거합니다.
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
    }
}