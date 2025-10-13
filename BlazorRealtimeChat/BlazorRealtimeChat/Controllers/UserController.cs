using System;
using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService, ITokenService tokenService) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        var (success, errorMessage) = await userService.RegisterUserAsync(userDto);

        if (!success)
        {
            return BadRequest(new { Error = errorMessage });
        }

        return Ok(new { Message = "회원가입이 성공적으로 완료되었습니다." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        var user = await userService.LoginUserAsync(userDto);

        // 1. 사용자 비밀번호 검증
        if (user == null)
        {
            return Unauthorized(new { Error = "사용자 이름 또는 비밀번호가 잘못되었습니다." });
        }

        // 2. 토큰 생성
        var accessToken = tokenService.GenerateTokenAsync(user);
        var refreshToken = tokenService.GenrateRefreshTokenAsync();

        // 3. Refresh Token을 DB에 저장 (나중에 검증을 위해)
        // 예: await userService.SaveRefreshTokenAsync(user.UserId, refreshToken);
        
        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        });
        
    }

}
