using System;
using BlazorRealtimeChat.Data.Entity;
using BlazorRealtimeChat.Repositories;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    
    public async Task<(bool Success, string ErrorMessage)> RegisterUserAsync(UserRegisterDto userDto)
    {
        // 1. 사용자 이름 중복 확인
        var existingUser = await userRepository.GetUserByUsernameAsync(userDto.UserName);
        if (existingUser != null)
        {
            return (false, "사용자 이름 중복 확인");
        }

        // 2. 사용자 아이디 중복 확인
        var existingLoginId = await userRepository.GetUserByLoginIdAsync(userDto.LoginId);
        if (existingLoginId != null)
        {
            return (false, "사용자 아이디 중복");
        }

        // 2. 비밀번호 해싱
        // Bycrpt.Net 사용
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

        var newUser = new User
        {
            LoginId = userDto.LoginId,
            UserName = userDto.UserName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        // 3. 사용자 정보 저장
        await userRepository.AddUserAsync(newUser);


        return (true, "");
    }

    public async Task<User?> LoginUserAsync(UserLoginDto userDto)
    {
        // 1. 사용자 아이디로 사용자 정보 조회
        var user = await userRepository.GetUserByLoginIdAsync(userDto.LoginId);
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

    public async Task<(bool Success, string ErrorMessage)> UpdateProfileImageAsync(Guid id, string imageUrl)
    {
        var user = await userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return (false, "User not found.");
        }
        user.ProfileImageUrl = imageUrl;
        await userRepository.UpdateProfileImgeAsync(id, imageUrl);
        return (true, "");
    }

    public async Task<UserDto?> GetUserByUserIdAsncy(Guid id)
    {
        var user = await userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            LoginId = user.LoginId,
            UserName = user.UserName,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    public async Task<ServiceResponse<string>> UpdateUserAsync(Guid id,UpdateUserDto updateUserDto)
    {
        var response = new ServiceResponse<string>();

        var user = await userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            response.Success = false;
            response.Status = ResponseStatus.NotFound;
            response.Message = "사용자를 찾을 수 없습니다.";
            return response;
        }

        user.UserName = updateUserDto.UserName;
        await userRepository.UpdateUserAsync(user);

        response.Data = user.UserName;
        return response;
    }
}
