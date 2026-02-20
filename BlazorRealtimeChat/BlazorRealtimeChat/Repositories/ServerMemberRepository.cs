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
}
