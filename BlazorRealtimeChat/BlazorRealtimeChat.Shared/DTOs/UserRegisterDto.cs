using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Shared.DTOs;

public class UserRegisterDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
