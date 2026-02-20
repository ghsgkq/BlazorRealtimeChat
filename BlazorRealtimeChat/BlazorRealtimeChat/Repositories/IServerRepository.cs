using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Repositories;

public interface IServerRepository
{
    Task<IEnumerable<Server>> GetServersAsync(Guid userId);
    Task<Server> AddServerAsync(Server server);
}