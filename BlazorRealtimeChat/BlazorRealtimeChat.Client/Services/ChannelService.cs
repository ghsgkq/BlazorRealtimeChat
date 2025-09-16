using System;
using System.Net.Http.Json;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public class ChannelService : IChannelService
{
    private readonly HttpClient _httpClient;

        public ChannelService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ChannelDto>> GetChannelsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ChannelDto>>("api/channel");
        }

        public async Task<ChannelDto> CreateChannelAsync(CreateChannelDto newChannel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/channel", newChannel);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChannelDto>();
            }
            return null; // 실패 시 null 반환
        }
}
