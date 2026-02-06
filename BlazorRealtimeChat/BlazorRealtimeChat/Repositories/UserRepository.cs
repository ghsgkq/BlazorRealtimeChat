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

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users.FindAsync(userId);
    }

    public async Task UpdateProfileImgeAsync(Guid userId, string profileImageUrl)
    {
        var user = await context.Users.FindAsync(userId);
        if (user != null)
        {
            Console.WriteLine($"사용자 찾음: {user.UserName}");
            user.ProfileImageUrl = profileImageUrl;
            var result = await context.SaveChangesAsync();
            Console.WriteLine($"변경 사항 저장 완료: {result}개 행 영향 받음");
        }
        else
        {
            Console.WriteLine($"사용자를 찾을 수 없음. ID: {userId}");
        }
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }
}
