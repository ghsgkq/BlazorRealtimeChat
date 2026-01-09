using BlazorRealtimeChat.Shared.DTOs;
using System.Net.Http.Json;

namespace BlazorRealtimeChat.Client.Services
{
    public class MessageService : IMessageService
    {
        private readonly HttpClient _httpClient;
        public MessageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<MessageDto>> GetMessagesByChannelIdAsync(Guid channelId, int limit = 50, int offset = 0)
        {
            return await _httpClient.GetFromJsonAsync<List<MessageDto>>($"api/messages/{channelId}?limit={limit}&offset={offset}");
        }
    }
}
