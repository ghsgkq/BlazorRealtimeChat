using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services
{
    public interface IMessageService
    {
        Task<List<MessageDto>> GetMessagesByChannelIdAsync(Guid channelId, int limit = 50, int offset = 0);
    }
}
