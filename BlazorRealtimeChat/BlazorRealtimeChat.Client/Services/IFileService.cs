using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorRealtimeChat.Client.Services
{
    public interface IFileService
    {
        Task<FileDto> UploadFileAsync(IBrowserFile file);
    }
}
