using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services
{
    public class MessageService(IMessageRepository messageRepository) : IMessageService
    {
        public async Task<IEnumerable<MessageDto>> GetMessageByChannelIdAsync(Guid channelId, int limit, int offset)
        {
            var messages = await messageRepository.GetMessageByChannelIdAsync(channelId, limit, offset);
            return messages.Select(m => new MessageDto
            {
                MessageId = m.MessageId,
                ChannelId = m.ChannelId,
                UserId = m.User.UserId,
                Content = m.Content,
                Timestamp = m.Timestamp,
                ChannelName = m.Channel.ChannelName,
                UserName = m.User.UserName,
                FileUrl = m.FileUrl,
                MessageType = m.MessageType,
                ProfileImageUrl = m.User.ProfileImageUrl
            });
        }
    }
}
