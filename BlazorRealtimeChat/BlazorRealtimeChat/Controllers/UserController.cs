using System;
using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        var (success, errorMessage) = await _userService.RegisterUserAsync(userDto);

        if (!success)
        {
            return BadRequest(new { Error = errorMessage });
        }

        return Ok(new { Message = "회원가입이 성공적으로 완료되었습니다." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        var user = await _userService.LoginUserAsync(userDto);

        // 1. 사용자 비밀번호 검증
        if (user == null)
        {
            return Unauthorized(new { Error = "사용자 이름 또는 비밀번호가 잘못되었습니다." });
        }

        // 2. 토큰 생성
        var accessToken = _tokenService.GenerateTokenAsync(user);
        var refreshToken = _tokenService.GenrateRefreshTokenAsync();

        // 3. Refresh Token을 DB에 저장 (나중에 검증을 위해)
        // 예: await _userService.SaveRefreshTokenAsync(user.UserId, refreshToken);
        
        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        });
        
    }

}
