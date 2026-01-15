using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorRealtimeChat.Client.Services
{
    public interface IFileService
    {
        Task<FileDto> UploadFileAsync(IBrowserFile file);
        // 스트림을 직접 받는 메서드
        Task<FileDto> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    }
}
