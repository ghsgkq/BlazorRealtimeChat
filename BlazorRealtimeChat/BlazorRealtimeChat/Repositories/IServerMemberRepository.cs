using System;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public interface IServerMemberRepository
{
    Task AddServerMemberAsync(ServerMember serverMember);
}
