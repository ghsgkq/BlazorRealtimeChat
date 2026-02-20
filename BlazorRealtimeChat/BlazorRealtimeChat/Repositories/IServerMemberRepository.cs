using System;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public interface IServerMemberRepository
{
    Task AddServerMemberAsync(ServerMember serverMember);

    Task<bool> IsMemberAsync(Guid serverId, Guid userId);
}
