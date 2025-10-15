using BlazorRealtimeChat.Shared.DTOs;
using System.Net.Http.Json;

namespace BlazorRealtimeChat.Client.Services;

public class ServerService : IServerService
{
    private readonly HttpClient _httpClient;

    public ServerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ServerDto>> GetServersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ServerDto>>("api/servers");
    }

    public async Task<ServerDto> CreateServerAsync(CreateServerDto createServerDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/servers", createServerDto);
        response.EnsureSuccessStatusCode(); // 실패 시 예외 발생
        return await response.Content.ReadFromJsonAsync<ServerDto>();
    }
}
