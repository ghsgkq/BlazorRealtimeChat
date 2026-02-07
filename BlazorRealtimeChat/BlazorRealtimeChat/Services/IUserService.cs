using System;
using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public interface IUserService
{
    Task<(bool Success, string ErrorMessage)> RegisterUserAsync(UserRegisterDto userDto);
    Task<User?> LoginUserAsync(UserLoginDto userDto);
    Task<(bool Success, string ErrorMessage)> UpdateProfileImageAsync(Guid id, string imageUrl);
    Task<UserDto?> GetUserByUserIdAsncy(Guid id);
    Task<ServiceResponse<string>> UpdateUserAsync(Guid id,UpdateUserDto updateUserDto);
}
