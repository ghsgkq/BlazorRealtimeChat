using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> AddMessageAsync(Message message);
        Task<IEnumerable<Message>> GetMessageByChannelIdAsync(Guid channelId, int limit, int offset);

    }
}
