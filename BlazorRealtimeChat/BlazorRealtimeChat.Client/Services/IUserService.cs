using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public interface IUserService
{
    Task<(bool Success, string ErrorMessage)> UpdateProfileImageAsync(string imageUrl);
    Task<UserDto?> GetUserByUserIdAsync();
    Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto);
}

