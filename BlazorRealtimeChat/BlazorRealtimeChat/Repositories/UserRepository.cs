using System;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Repositories;

public class UserRepository(RealTimeChatContext context) : IUserRepository
{

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User> AddUserAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task UpdateProfileImgeAsync(Guid id, string profileImageUrl)
    {
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            user.ProfileImageUrl = profileImageUrl;
            var result = await context.SaveChangesAsync(); 
        }
        else
        {
            Console.WriteLine($"사용자를 찾을 수 없음. ID: {id}");
        }
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByLoginIdAsync(string loginId)
    { 
        return await context.Users.FirstOrDefaultAsync(u => u.LoginId == loginId);
    }

}
