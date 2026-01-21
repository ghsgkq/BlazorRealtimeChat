using BlazorRealtimeChat.Shared.Enums;
using System;

namespace BlazorRealtimeChat.Shared.DTOs;

public class ChannelDto
{
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = null!;

    public ChannelType Type { get; set; }
}
