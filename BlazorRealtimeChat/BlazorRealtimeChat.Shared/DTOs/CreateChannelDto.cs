using BlazorRealtimeChat.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Shared.DTOs;

public class CreateChannelDto
{
    [Required]
    public Guid ServerId { get; set; }

    [Required]
    [MaxLength(100, ErrorMessage = "채널 이름은 100자를 초과할 수 없습니다.")]
    [MinLength(2, ErrorMessage = "채널 이름은 최소 2자 이상이어야 합니다.")]
    public string ChannelName { get; set; }

    [Required]
    public ChannelType Type { get; set; } = ChannelType.Text;
}
