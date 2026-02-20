using System;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public class ServerMemberRepository(RealTimeChatContext context) : IServerMemberRepository
{
    public async Task AddServerMemberAsync(ServerMember serverMember)
    {
        context.ServerMembers.Add(serverMember);
        await context.SaveChangesAsync();
    }
}
