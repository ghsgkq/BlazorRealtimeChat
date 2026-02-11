using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public interface IServerService
{
    Task<IEnumerable<ServerDto>> GetServersAsync();
    Task<ServerDto> AddServerAsync(CreateServerDto createServerDto, Guid ownerId);
    
}