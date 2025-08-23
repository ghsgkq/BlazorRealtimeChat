using System;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<bool> LoginAsync(string userName, string password)
    {
        var userDto = new UserLoginDto
        {
            Username = userName,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("api/user/login", userDto);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();

        if (result is null)
        {
            return false;
        }

        await _localStorage.SetItemAsync("accessToken", result.AccessToken);
        // Refresh Token은 안전하게 Local Storage에 저장 (Access Token 재발급 시 사용)
        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

        // Access Token은 HttpClient의 기본 헤더에 설정 (API 요청 시마다 사용)
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

        return true;
    }
    
    public async Task LogoutAsync()
    {
        // HttpClient 헤더와 Local Storage에서 토큰 정보 제거
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await _localStorage.RemoveItemAsync("refreshToken");
    }
}
