using System;
using System.Net.Http.Json;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Client.Services;

public class RegisterService : IRegisterService
{
    private readonly HttpClient _httpClient;
    
    public RegisterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<bool> RegisterAsync(string userName, string password, string confirmPassword)
    {

        // Simple validation logic for demonstration purposes
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return false; // Registration failed due to empty fields
        }

        if (password != confirmPassword)
        {
            return false; // Registration failed due to password mismatch
        }

        var registerDto = new UserRegisterDto
        {
            Username = userName,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("api/user/register", registerDto);
        if (!response.IsSuccessStatusCode)
        {
            return false; // Registration failed due to server error
        }
        return true; // Registration successful
    }
}
