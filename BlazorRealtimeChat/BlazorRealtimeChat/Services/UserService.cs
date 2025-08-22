using System;
using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterUserAsync(UserRegisterDto userDto)
    {
        // 1. 사용자 이름 중복 확인
        var existingUser = await _userRepository.GetUserByUsernameAsync(userDto.Username);
        if (existingUser != null)
        {
            return (false, "Username already exists.");
        }

        // 2. 비밀번호 해싱
        // Bycrpt.Net 사용
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

        var newUser = new User
        {
            UserName = userDto.Username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        // 3. 사용자 정보 저장
        await _userRepository.AddUserAsync(newUser);


        return (true, "");
    }

    public async Task<User?> LoginUserAsync(UserLoginDto userDto)
    {
        // 1. 사용자 이름으로 사용자 정보 조회
        var user = await _userRepository.GetUserByUsernameAsync(userDto.Username);
        if (user == null)
        {
            return null; // 사용자 없음
        }

        // 2. 비밀번호 검증
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null; // 비밀번호 불일치
        }
        
        return user;
    }
}
