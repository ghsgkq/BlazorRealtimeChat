using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorRealtimeChat.Client.Services
{
    public interface IFileService
    {
        // IBrowserFile은 내부에 .Size 속성이 있어 totalSize가 필요 없음
        Task<FileDto> UploadFileAsync(IBrowserFile file, Action<long, long>? onProgress = null);

        // 스트림을 직접 받을 때는 전체 크기를 알아야 퍼센트 계산이 가능함
        Task<FileDto> UploadFileAsync(Stream fileStream, string fileName, string contentType, long totalSize, Action<long, long>? onProgress = null);
    }
}
