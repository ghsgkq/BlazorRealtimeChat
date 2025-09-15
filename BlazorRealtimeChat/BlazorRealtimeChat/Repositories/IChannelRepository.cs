using System;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public interface IChannelRepository
{
    Task<IEnumerable<Channel>> GetChannelsAsync();
    Task<Channel> AddChannelAsync(Channel channel);
}
