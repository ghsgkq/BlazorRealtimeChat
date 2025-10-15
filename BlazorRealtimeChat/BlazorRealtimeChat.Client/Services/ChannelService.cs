using BlazorRealtimeChat.Shared.DTOs;
using System.Net.Http.Json;

namespace BlazorRealtimeChat.Client.Services;

public class ChannelService : IChannelService
{
    private readonly HttpClient _httpClient;

    public ChannelService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // serverId를 사용하여 동적으로 API 경로를 생성합니다.
    public async Task<List<ChannelDto>> GetChannelsAsync(Guid serverId)
    {
        return await _httpClient.GetFromJsonAsync<List<ChannelDto>>($"api/servers/{serverId}/channels");
    }

    // 채널 생성 시에도 올바른 경로를 사용해야 합니다.
    public async Task<ChannelDto> CreateChannelAsync(CreateChannelDto createChannelDto)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/servers/{createChannelDto.ServerId}/channels", createChannelDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChannelDto>();
    }
}
