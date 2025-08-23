using System;

namespace BlazorRealtimeChat.Client.Services;

public interface IRegisterService
{
    public Task<bool> RegisterAsync(string userName, string password, string confirmPassword);
}
