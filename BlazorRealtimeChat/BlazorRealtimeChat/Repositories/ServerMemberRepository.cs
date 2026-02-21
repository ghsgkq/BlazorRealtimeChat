using System;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories;

public class ServerMemberRepository(RealTimeChatContext context) : IServerMemberRepository
{
    public async Task AddServerMemberAsync(ServerMember serverMember)
    {
        context.ServerMembers.Add(serverMember);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsMemberAsync(Guid serverId, Guid userId)
    {
        // 이미 이 서버의 멤버인지 확인합니다.
        return await context.ServerMembers
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId);
    }
    
    public async Task<IEnumerable<User>> GetMembersByServerIdAsync(Guid serverId)
    {
        // ServerMembers 테이블에서 ServerId가 일치하는 유저 정보(User)만 쏙 뽑아옵니다.
        return await context.ServerMembers
            .Where(sm => sm.ServerId == serverId)
            .Include(sm => sm.User) 
            .Select(sm => sm.User)
            .ToListAsync();
    }
}
