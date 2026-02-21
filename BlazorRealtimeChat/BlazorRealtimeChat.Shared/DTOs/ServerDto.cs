namespace BlazorRealtimeChat.Shared.DTOs;

public class ServerDto
{
    public Guid ServerId { get; set; }
    public string ServerName { get; set; } = null!;
    public Guid OwnerId { get; set; } // 소유주 확인용

    // 서버 프로필
    public string? ProfileImageUrl {get; set;}
}
