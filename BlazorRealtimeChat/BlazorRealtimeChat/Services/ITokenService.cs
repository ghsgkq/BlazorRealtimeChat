using System;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Services;

public interface ITokenService
{
    string GenerateTokenAsync(User user);
    string GenrateRefreshTokenAsync();
}
