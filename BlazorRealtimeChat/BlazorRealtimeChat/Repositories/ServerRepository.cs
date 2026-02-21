using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories;

public class ServerRepository(RealTimeChatContext context) : IServerRepository
{
    public async Task<IEnumerable<Server>> GetServersAsync(Guid userId)
    {
        // 초대된 서버를 이름순으로 정렬하여 반환
        return await context.Servers
            .Where(s => s.OwnerId == userId || 
                        context.ServerMembers.Any(sm => sm.ServerId == s.ServerId && sm.UserId == userId))
            .OrderBy(s => s.ServerName)
            .ToListAsync();
    }

    public async Task<Server> AddServerAsync(Server server)
    {
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        return server;
    }

    public async Task<Server?> GetServerByIdAsync(Guid serverId)
    {
        return await context.Servers.FirstOrDefaultAsync(s => s.ServerId == serverId);
    }

    public async Task<Server> UpdateServerAsync(Server server)
    {
        context.Servers.Update(server);
        await context.SaveChangesAsync();
        return server;
    }

    public async Task<bool> DeleteServerAsync(Guid serverId)
    {
        var server = await context.Servers.FindAsync(serverId);
        if (server == null) return false;

        context.Servers.Remove(server);
        await context.SaveChangesAsync(); 
        // Entity Framework의 'Cascade Delete(연쇄 삭제)' 기능 덕분에 
        // 서버에 속한 채널, 메시지, 멤버 기록들도 자동으로 깔끔하게 지워집니다!
        return true;
    }
}