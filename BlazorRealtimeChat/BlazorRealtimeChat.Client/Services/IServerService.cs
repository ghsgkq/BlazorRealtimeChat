using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public interface IServerService
{
    Task<List<ServerDto>> GetServersAsync();
    Task<ServerDto> CreateServerAsync(CreateServerDto createServerDto);

    Task<bool> JoinServerAsync(Guid serverId);
    Task<ServerPreviewDto?> GetServerPreviewAsync(Guid serverId);
}
