using System;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

    public interface IChannelService
    {
        Task<List<ChannelDto>> GetChannelsAsync();
        Task<ChannelDto> CreateChannelAsync(CreateChannelDto newChannel);
    }