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

    public async Task<bool> JoinServerAsync(Guid serverId)
    {
        // 백엔드의 POST api/servers/{serverId}/join 엔드포인트 호출
        var response = await _httpClient.PostAsync($"api/servers/{serverId}/join", null);
        
        // 성공(200 OK)했다면 true 반환, 아니면 false 반환
        return response.IsSuccessStatusCode;
    }

    public async Task<ServerPreviewDto?> GetServerPreviewAsync(Guid serverId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ServerPreviewDto>($"api/servers/{serverId}/preview");
        }
        catch
        {
            return null;
        }
    }
}
