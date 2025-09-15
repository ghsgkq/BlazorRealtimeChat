using System;

namespace BlazorRealtimeChat.Shared.DTOs;

public class ChannelDto
{
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = null!; 
}
