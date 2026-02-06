using BlazorRealtimeChat.Shared.DTOs;
using System.Net.Http.Json;

namespace BlazorRealtimeChat.Client.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateProfileImageAsync(string imageUrl)
        {

            var response = await _httpClient.PatchAsJsonAsync("api/user/update-profile", imageUrl);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
        }

        public async Task<UserDto?> GetUserByUserIdAsync()
        {
            return await _httpClient.GetFromJsonAsync<UserDto>("api/user/me");
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/user", updateUserDto);
            return response.IsSuccessStatusCode;
        }

    }
}
