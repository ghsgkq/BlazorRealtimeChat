using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Repositories;

public interface IServerRepository
{
    Task<IEnumerable<Server>> GetServersAsync();
    Task<Server> AddServerAsync(Server server);
}