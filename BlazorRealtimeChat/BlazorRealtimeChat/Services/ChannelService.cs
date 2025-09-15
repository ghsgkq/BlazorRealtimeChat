using System;
using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepository;

    public ChannelService(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<IEnumerable<ChannelDto>> GetChannelsAsync()
    {
        var channels = await _channelRepository.GetChannelsAsync();

        // 데이터베이스 엔티티(Channel)를 프론트엔드용 DTO(ChannelDto)로 변환합니다.
        return channels.Select(channel => new ChannelDto
        {
            ChannelId = channel.ChannelId,
            ChannelName = channel.ChannelName
        });
    }
    
    public async Task<ChannelDto> AddChannelAsync(CreateChannelDto createChannelDto)
    {
        var newChannel = new Data.Entity.Channel
        {
            ChannelName = createChannelDto.ChannelName,
            ServerId = createChannelDto.ServerId
        };

        var createdChannel = await _channelRepository.AddChannelAsync(newChannel);

        return new ChannelDto
        {
            ChannelId = createdChannel.ChannelId,
            ChannelName = createdChannel.ChannelName
        };
    }

}
