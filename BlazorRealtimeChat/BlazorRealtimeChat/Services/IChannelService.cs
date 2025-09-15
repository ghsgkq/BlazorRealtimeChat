using System;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public interface IChannelService
{
    Task<IEnumerable<ChannelDto>> GetChannelsAsync();
    Task<ChannelDto> AddChannelAsync(CreateChannelDto createChannelDto);
}
