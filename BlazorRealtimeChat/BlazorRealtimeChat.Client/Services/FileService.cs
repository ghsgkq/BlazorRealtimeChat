using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Headers;
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

        // 1. IBrowserFile 전용 업로드
        public async Task<FileDto> UploadFileAsync(IBrowserFile file, Action<long, long>? onProgress = null)
        {
            var maxAllowedSize = 100 * 1024 * 1024; // 100MB
            var stream = file.OpenReadStream(maxAllowedSize);

            return await UploadFileAsync(stream, file.Name, file.ContentType, file.Size, onProgress);
        }

        // 2. 공통 스트림 업로드 (진행률 추적 로직 포함)
        public async Task<FileDto> UploadFileAsync(Stream fileStream, string fileName, string contentType, long totalSize, Action<long, long>? onProgress = null)
        {
            using var content = new MultipartFormDataContent();

            // [핵심] 일반 StreamContent 대신 진행률을 보고하는 커스텀 클래스 사용
            var progressContent = new ProgressStreamContent(fileStream, totalSize, (sent, total) =>
            {
                onProgress?.Invoke(sent, total);
            });

            progressContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(progressContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/files/upload", content);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<FileDto>())!;
        }

        // --- 내부 헬퍼 클래스 (진행률 가로채기) ---
        private class ProgressStreamContent : HttpContent
        {
            private readonly Stream _innerStream;
            private readonly long _totalSize;
            private readonly Action<long, long> _onProgress;
            private const int BufferSize = 10240; // 10KB 단위로 읽기

            public ProgressStreamContent(Stream innerStream, long totalSize, Action<long, long> onProgress)
            {
                _innerStream = innerStream;
                _totalSize = totalSize;
                _onProgress = onProgress;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                var buffer = new byte[BufferSize];
                long totalSent = 0;

                while (true)
                {
                    var bytesRead = await _innerStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    await stream.WriteAsync(buffer, 0, bytesRead);
                    totalSent += bytesRead;

                    // 진행률 콜백 실행
                    _onProgress(totalSent, _totalSize);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _totalSize;
                return true;
            }
        }
    }
}
