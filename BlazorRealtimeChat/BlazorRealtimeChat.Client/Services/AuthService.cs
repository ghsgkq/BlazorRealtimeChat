using System;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorRealtimeChat.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
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

        if (result is null || string.IsNullOrEmpty(result.AccessToken))
        {
            return false;
        }

        await _localStorage.SetItemAsync("accessToken", result.AccessToken);
        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

        ((CustomAuthStateProvider)_authenticationStateProvider).NotifyUserAuthentication(result.AccessToken);

        return true;
    }
    
    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        ((CustomAuthStateProvider)_authenticationStateProvider).NotifyUserLogout();
    }
}
