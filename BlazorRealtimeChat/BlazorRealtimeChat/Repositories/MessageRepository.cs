using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories
{
    public class MessageRepository(RealTimeChatContext context) : IMessageRepository
    {
        public async Task<Message> AddMessageAsync(Message message)
        {
            context.Messages.Add(message);
            await context.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<Message>> GetMessageByChannelIdAsync(Guid channelId, int limit, int offset)
        {
            return await context.Messages
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .Include(m => m.Channel)
                .OrderByDescending(m => m.Timestamp)
                .Skip(offset)
                .Take(limit)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
