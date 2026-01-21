using System;
using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public class ChannelService(IChannelRepository channelRepository) : IChannelService
{


    public async Task<IEnumerable<ChannelDto>> GetChannelsAsync(Guid serverId)
    {
        var channels = await channelRepository.GetChannelsAsync(serverId);

        // 데이터베이스 엔티티(Channel)를 프론트엔드용 DTO(ChannelDto)로 변환합니다.
        return channels.Select(channel => new ChannelDto
        {
            ChannelId = channel.ChannelId,
            ChannelName = channel.ChannelName,
            Type = channel.Type
        });
    }
    
    public async Task<ChannelDto> AddChannelAsync(CreateChannelDto createChannelDto)
    {
        var newChannel = new Data.Entity.Channel
        {
            ChannelName = createChannelDto.ChannelName,
            ServerId = createChannelDto.ServerId,
            Type = createChannelDto.Type
        };

        var createdChannel = await channelRepository.AddChannelAsync(newChannel);

        return new ChannelDto
        {
            ChannelId = createdChannel.ChannelId,
            ChannelName = createdChannel.ChannelName
        };
    }

}
