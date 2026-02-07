using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Shared.DTOs;

public class UserLoginDto
{
    [Required]
    public string LoginId { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
