using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public interface IServerService
{
    Task<IEnumerable<ServerDto>> GetServersAsync(Guid userId);
    Task<ServerDto> AddServerAsync(CreateServerDto createServerDto, Guid ownerId);
    Task<bool> JoinServerAsync(Guid serverId, Guid userId);   
    Task<ServerPreviewDto?> GetServerPreviewAsync(Guid serverId, Guid userId);
}