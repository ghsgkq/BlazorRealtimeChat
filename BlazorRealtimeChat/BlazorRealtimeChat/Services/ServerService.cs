using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public class ServerService(
    IServerRepository serverRepository, 
    IChannelRepository channelRepository,
    IServerMemberRepository serverMemberRepository) : IServerService
{
    public async Task<IEnumerable<ServerDto>> GetServersAsync(Guid userId)
    {
        var servers = await serverRepository.GetServersAsync(userId);
        
        // 데이터베이스 엔티티(Server)를 프론트엔드 DTO(ServerDto)로 반환합니다.
        return servers.Select(server => new ServerDto()
        {
            ServerId = server.ServerId,
            ServerName = server.ServerName,
            OwnerId = server.OwnerId
        });
    }

    // 서버 생성
    public async Task<ServerDto> AddServerAsync(CreateServerDto createServerDto, Guid ownerId)
    {
        // 1. 서버 생성
        var newServer = new Data.Entity.Server
        {
            ServerName = createServerDto.ServerName,
            OwnerId = ownerId
        };

        var createdServer = await serverRepository.AddServerAsync(newServer);

        var ownerMember = new Data.Entity.ServerMember
        {
            ServerId = createdServer.ServerId,
            UserId = ownerId  
        };

        // 2. 서버 자동 초대
        await serverMemberRepository.AddServerMemberAsync(ownerMember);

        // 3. 기본 채널 추가하기
        var defaultChannel = new Data.Entity.Channel
        {
            ChannelName = "일반",
            ServerId = createdServer.ServerId
        };
        await channelRepository.AddChannelAsync(defaultChannel);

        return new ServerDto
        {
            ServerId = createdServer.ServerId,
            ServerName = createdServer.ServerName,
            OwnerId = createdServer.OwnerId

        };
    }

    public async Task<bool> JoinServerAsync(Guid serverId, Guid userId)
    {
        // 1. 서버가 실제로 존재하는지 확인합니다.
        var server = await serverRepository.GetServerByIdAsync(serverId);
        if (server == null) return false;

        // 2. 이미 가입된 멤버인지 확인합니다. (중복 에러 방지)
        var isAlreadyMember = await serverMemberRepository.IsMemberAsync(serverId, userId);
        if (isAlreadyMember) return true; // 이미 멤버라면 성공(true)으로 넘깁니다.

        // 3. 서버 멤버 엔티티를 생성하고 저장합니다.
        var newMember = new Data.Entity.ServerMember
        {
            ServerId = serverId,
            UserId = userId
        };
        
        await serverMemberRepository.AddServerMemberAsync(newMember);
        
        return true;
    }
}