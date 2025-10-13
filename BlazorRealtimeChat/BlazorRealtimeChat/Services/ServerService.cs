using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public class ServerService(IServerRepository serverRepository) : IServerService
{


    public async Task<IEnumerable<ServerDto>> GetServersAsync()
    {
        var servers = await serverRepository.GetServersAsync();
        
        // 데이터베이스 엔티티(Server)를 프론트엔드 DTO(ServerDto)로 반환합니다.
        return servers.Select(server => new ServerDto()
        {
            ServerId = server.ServerId,
            ServerName = server.ServerName
        });
    }
    
    
}