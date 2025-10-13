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
}
