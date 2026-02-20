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
            ServerName = server.ServerName
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
}