using System;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories;

public class ChannelRepository(RealTimeChatContext context) : IChannelRepository
{
    public async Task<IEnumerable<Channel>> GetChannelsAsync()
    {
        // 모든 채널을 이름순으로 정렬하여 반환합니다.
        return await context.Channels
            .OrderBy(c => c.ChannelName)
            .ToListAsync();
    }

    public async Task<Channel> AddChannelAsync(Channel channel)
    {
        context.Channels.Add(channel);
        await context.SaveChangesAsync();
        return channel;
    }
    
}
