using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorRealtimeChat.Shared.DTOs;

namespace BlazorRealtimeChat.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/files")]
    public class FileController(IWebHostEnvironment env) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("파일이 없습니다.");

            // [수정] wwwroot가 아닌 프로젝트 루트의 "ExternalUploads" 폴더를 사용합니다.
            // 이렇게 하면 비주얼 스튜디오 감시자가 파일을 감지하지 못합니다.
            var externalPath = Path.Combine(Directory.GetCurrentDirectory(), "ExternalUploads");

            if (!Directory.Exists(externalPath))
                Directory.CreateDirectory(externalPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(externalPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            FileDto fileDto = new()
            {
                FileName = fileName,
                FileUrl = $"/user-files/{fileName}" // URL 경로를 /user-files로 설정
            };

            return Ok(fileDto);
        }
    }
}
