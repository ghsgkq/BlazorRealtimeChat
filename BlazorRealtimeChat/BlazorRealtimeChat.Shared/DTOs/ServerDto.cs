namespace BlazorRealtimeChat.Shared.DTOs;

public class ServerDto
{
    public Guid ServerId { get; set; }
    public string ServerName { get; set; } = null!;
}