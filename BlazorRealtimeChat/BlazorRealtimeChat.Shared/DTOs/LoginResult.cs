using System;

namespace BlazorRealtimeChat.Shared.DTOs;

public class LoginResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
