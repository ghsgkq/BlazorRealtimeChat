using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Data.Entity;

public class User
{
    [Key]
    // 1. 시스템 내부에서 관계를 맺을 때 사용하는 고유 키 (PK)
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    // 2. 로그인할 때 사용하는 계정 아이디 (변경 불가능하거나 까다롭게 관리)
    // 예: @unique_id
    public string LoginId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    // 3. 실제 채팅창이나 프로필에 노출되는 이름 (언제든 변경 가능)
    // 예: "별명", "닉네임"
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 관계 설정
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
