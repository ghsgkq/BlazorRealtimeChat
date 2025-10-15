using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories;

public class ServerRepository(RealTimeChatContext context) : IServerRepository
{
    public async Task<IEnumerable<Server>> GetServersAsync()
    {
        // 모든 서버를 이름순으로 정렬하여 반환
        return await context.Servers.OrderBy(s => s.ServerName).ToListAsync();
    }

    public async Task<Server> AddServerAsync(Server server)
    {
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        return server;
    }
}