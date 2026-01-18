namespace BlazorRealtimeChat.Client.Services;

public interface IUserService
{
    Task<(bool Success, string ErrorMessage)> UpdateProfileImageAsync(string imageUrl);
}

