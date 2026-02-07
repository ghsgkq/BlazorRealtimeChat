using System;

namespace BlazorRealtimeChat.Client.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string loginId, string password);
    Task LogoutAsync();
}
