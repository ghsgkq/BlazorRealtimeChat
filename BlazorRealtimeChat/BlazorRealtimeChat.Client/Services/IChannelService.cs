using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public interface IChannelService
{
    // 이제 serverId를 파라미터로 받아서 해당 서버의 채널만 가져옵니다.
    Task<List<ChannelDto>> GetChannelsAsync(Guid serverId);
    Task<ChannelDto> CreateChannelAsync(CreateChannelDto createChannelDto);
}
