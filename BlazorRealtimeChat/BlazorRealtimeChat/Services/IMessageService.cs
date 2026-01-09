using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetMessageByChannelIdAsync(Guid channelId, int limit, int offset);
    }
}
