using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public interface IServerService
{
    Task<List<ServerDto>> GetServersAsync();
    Task<ServerDto> CreateServerAsync(CreateServerDto createServerDto);

    Task<bool> JoinServerAsync(Guid serverId);
    Task<ServerPreviewDto?> GetServerPreviewAsync(Guid serverId);

    Task<IEnumerable<ServerMemberDto>> GetServerMembersAsync(Guid serverId);

    Task<ServerDto?> UpdateServerAsync(Guid serverId, UpdateServerDto updateDto);
}
