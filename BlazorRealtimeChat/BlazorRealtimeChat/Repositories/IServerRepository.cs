using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Repositories;

public interface IServerRepository
{
    Task<IEnumerable<Server>> GetServersAsync(Guid userId);
    Task<Server> AddServerAsync(Server server);
    Task<Server?> GetServerByIdAsync(Guid serverId);

    Task<Server> UpdateServerAsync(Server server);

    Task<bool> DeleteServerAsync(Guid serverId);
}