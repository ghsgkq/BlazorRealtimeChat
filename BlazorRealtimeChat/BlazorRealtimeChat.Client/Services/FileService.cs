using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace BlazorRealtimeChat.Client.Services
{
    public class FileService : IFileService
    {
        private readonly HttpClient _httpClient;
        public FileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<FileDto> UploadFileAsync(IBrowserFile file)
        {
            // [중요] using을 사용하여 전송 완료 후 MultipartFormDataContent를 즉시 해제합니다.
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync("api/files/upload", content);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<FileDto>())!;
        }
        public async Task<FileDto> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            // 전달받은 스트림을 직접 StreamContent로 변환
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/files/upload", content);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<FileDto>())!;

        }
    }
}
