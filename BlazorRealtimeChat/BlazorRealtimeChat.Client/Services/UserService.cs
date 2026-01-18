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
    }
}
