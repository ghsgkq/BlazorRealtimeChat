using System;
using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public interface IUserService
{
    Task<(bool Success, string ErrorMessage)> RegisterUserAsync(UserRegisterDto userDto);
    Task<User?> LoginUserAsync(UserLoginDto userDto);
    Task<(bool Success, string ErrorMessage)> UpdateProfileImageAsync(Guid userId, string imageUrl);
    Task<UserDto?> GetUserByUserIdAsncy(Guid userId);
}
