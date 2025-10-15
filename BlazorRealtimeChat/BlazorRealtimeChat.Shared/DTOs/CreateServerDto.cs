using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Shared.DTOs;

public class CreateServerDto
{
    [Required]
    [MaxLength(100, ErrorMessage = "서버 이름은 100자를 초과할 수 없습니다.")]
    [MinLength(2, ErrorMessage = "서버 이름은 최소 2자 이상이어야 합니다.")]
    public string ServerName { get; set; }
    
}