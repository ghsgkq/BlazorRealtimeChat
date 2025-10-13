using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public interface IServerRepository
{
    Task<IEnumerable<Server>> GetServersAsync();
}