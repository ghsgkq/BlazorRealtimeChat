using System;
using BlazorRealtimeChat.Data.Entity;

namespace BlazorRealtimeChat.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User> AddUserAsync(User user);
    Task<User?> GetUserByIdAsync(Guid id);
    Task UpdateProfileImgeAsync(Guid id, string profileImageUrl);

    Task<User?> UpdateUserAsync(User user);

    Task<User?> GetUserByLoginIdAsync(string loginId);

}
