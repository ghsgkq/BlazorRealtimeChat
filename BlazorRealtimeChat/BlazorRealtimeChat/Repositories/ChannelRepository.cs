using System;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly RealTimeChatContext _context;

    public ChannelRepository(RealTimeChatContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Channel>> GetChannelsAsync()
    {
        // 모든 채널을 이름순으로 정렬하여 반환합니다.
        return await _context.Channels
            .OrderBy(c => c.ChannelName)
            .ToListAsync();
    }

    public async Task<Channel> AddChannelAsync(Channel channel)
    {
        _context.Channels.Add(channel);
        await _context.SaveChangesAsync();
        return channel;
    }
    
}
