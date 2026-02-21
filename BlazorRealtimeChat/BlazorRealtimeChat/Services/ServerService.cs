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
            OwnerId = server.OwnerId,
            ProfileImageUrl = server.ProfileImageUrl
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

    public async Task<ServerPreviewDto?> GetServerPreviewAsync(Guid serverId, Guid userId)
    {
        // 1. 서버 정보 가져오기
        var server = await serverRepository.GetServerByIdAsync(serverId);
        if (server == null) return null;

        // 2. 이 유저가 이미 가입했는지 확인
        var isMember = await serverMemberRepository.IsMemberAsync(serverId, userId);

        // 3. 미리보기 정보 반환
        return new ServerPreviewDto
        {
            ServerId = server.ServerId,
            ServerName = server.ServerName,
            IsAlreadyMember = isMember
        };
    }

    public async Task<IEnumerable<ServerMemberDto>> GetServerMembersAsync(Guid serverId)
    {
        var members = await serverMemberRepository.GetMembersByServerIdAsync(serverId);
        
        // 현재 ChatHub에 접속 중인 유저 ID 목록을 가져옵니다.
        var onlineUserIds = Hubs.ChatHub.OnlineUsers.Values.ToHashSet();

        return members.Select(user => new ServerMemberDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            ProfileImageUrl = user.ProfileImageUrl,
            // 💡 교집합 확인: 온라인 유저 목록에 이 사람의 ID가 있으면 true!
            IsOnline = onlineUserIds.Contains(user.Id.ToString())
        }).OrderByDescending(m => m.IsOnline).ThenBy(m => m.UserName).ToList(); 
        // (온라인 유저를 위로, 그다음 이름순 정렬)
    }

    public async Task<ServerDto?> UpdateServerAsync(Guid serverId, UpdateServerDto updateDto, Guid userId)
    {
        // 1. 수정할 서버가 실제로 존재하는지 가져옵니다.
        var server = await serverRepository.GetServerByIdAsync(serverId);
        if (server == null) return null;

        // 2. 핵심 보안: 요청한 사람이 서버 방장(Owner)인지 확인합니다.
        if (server.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("서버 방장만 설정을 변경할 수 있습니다.");
        }

        // 3. 넘어온 데이터로 서버 정보를 업데이트합니다.
        if (!string.IsNullOrWhiteSpace(updateDto.ServerName))
        {
            server.ServerName = updateDto.ServerName;
        }
        
        if (updateDto.ProfileImageUrl != null)
        {
            // 기존 이미지가 존재하고, 새로 바꿀 이미지와 주소가 다를 때만 삭제 진행
            if (!string.IsNullOrEmpty(server.ProfileImageUrl) && server.ProfileImageUrl != updateDto.ProfileImageUrl)
            {
                try
                {
                    var oldFileName = Path.GetFileName(server.ProfileImageUrl);
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ExternalUploads", oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                        Console.WriteLine($"[System] 기존 서버 프로필 교체로 인한 삭제 완료: {oldFileName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] 기존 서버 프로필 이미지 삭제 실패: {ex.Message}");
                }
            }

            // 삭제 시도를 마친 후, 새로운 이미지 URL로 덮어씌웁니다.
            server.ProfileImageUrl = updateDto.ProfileImageUrl;
        }

        // 4. DB에 저장 후 DTO로 변환하여 반환합니다.
        var updatedServer = await serverRepository.UpdateServerAsync(server);

        return new ServerDto
        {
            ServerId = updatedServer.ServerId,
            ServerName = updatedServer.ServerName,
            OwnerId = updatedServer.OwnerId,
            ProfileImageUrl = updatedServer.ProfileImageUrl
        };
    }
    
    public async Task<bool> DeleteServerAsync(Guid serverId, Guid userId)
    {
        var server = await serverRepository.GetServerByIdAsync(serverId);
        if (server == null) return false;

        // 💡 방장 권한 체크!
        if (server.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("서버 방장만 서버를 삭제할 수 있습니다.");
        }

        // 👇 3. [추가] 물리적 프로필 이미지 파일 삭제 로직
        if (!string.IsNullOrEmpty(server.ProfileImageUrl))
        {
            try
            {
                // URL (예: "/user-files/abc-123.jpg") 에서 순수 파일명("abc-123.jpg")만 추출합니다.
                var fileName = Path.GetFileName(server.ProfileImageUrl);
                
                // FileController와 똑같은 경로(ExternalUploads)를 조합합니다.
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ExternalUploads", fileName);

                // 해당 경로에 실제 파일이 존재하면 지워줍니다.
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    Console.WriteLine($"[System] 서버 프로필 이미지 삭제 완료: {fileName}");
                }
            }
            catch (Exception ex)
            {
                // 혹시 파일이 사용 중이거나 오류가 나도, DB 삭제는 마저 진행되도록 catch만 해둡니다.
                Console.WriteLine($"[Error] 서버 프로필 이미지 삭제 실패: {ex.Message}");
            }
        }

        return await serverRepository.DeleteServerAsync(serverId);
    }
}