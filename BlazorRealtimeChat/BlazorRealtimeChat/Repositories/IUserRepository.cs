using System;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User> AddUserAsync(User user);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task UpdateProfileImgeAsync(Guid userId, string profileImageUrl);

}
