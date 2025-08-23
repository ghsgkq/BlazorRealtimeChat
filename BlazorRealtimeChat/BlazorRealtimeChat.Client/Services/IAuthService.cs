using System;

namespace BlazorRealtimeChat.Client.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string userName, string password);
    Task LogoutAsync();
}
