using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Shared.DTOs;

public class UserRegisterDto
{
    [Required, MaxLength(50)]
    public string LoginId { get; set; } = string.Empty; // 고유 아이디

    [Required, MaxLength(50)]
    public string UserName { get; set; } = string.Empty; // 초기 닉네임

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
